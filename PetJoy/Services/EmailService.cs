using System.Net;
using System.Net.Mail;
using PetJoy.Models;

namespace PetJoy.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            using var smtpClient = new SmtpClient
            {
                Host = _config["EmailSettings:SmtpServer"],
                Port = int.Parse(_config["EmailSettings:Port"]),
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    _config["EmailSettings:Username"],
                    _config["EmailSettings:Password"]
                )
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_config["EmailSettings:From"], "PetJoy"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(to);
            
            await smtpClient.SendMailAsync(mail);
        }



        // Adoption Email
        public (string Subject, string Body) BuildAdoptionEmail(string fullName, string petName, string status)
        {
            string subject = $"Adoption Update";
            string message;

            if (status == "Accepted")
            {
                message = $@"
            <p>Congratulations! Your application to adopt <strong>{petName}</strong> has been approved.</p>
            <p>Please contact PetJoy within <strong>three days</strong> to arrange the adoption.</p>
            <p>If we do not hear from you within this period, your adoption will be considered withdrawn, and the pet will be offered to another applicant.</p>
            <p>Thank you for helping us provide a loving home for our animals.</p>";
            }

            else
            {
                message = $@"
            <p>We are truly sorry to inform you that your application to adopt <strong>{petName}</strong> has not been approved.</p>
            <p>Another applicant with higher priority has been selected, but we sincerely appreciate your interest and care.</p>
            <p>We encourage you to explore other pets available for adoption and to apply again if you find another pet that touches your heart.</p>
            <p>Thank you for your support!</p>";
            }

            string body = $@"
            <h3>Dear {fullName},</h3>
            {message}
            <p>Best regards,<br/>PetJoy Team 🐾</p>";

            return (subject, body);
        }




        // Order Confirmation Email
        public string BuildOrderConfirmationEmail(CheckOut order)
        {
            var itemsHtml = string.Join("", order.OrderItems.Select(item => $@"
                <tr>
                    <td>{item.ShopProduct?.Name}</td>
                    <td>{item.Quantity}</td>
                    <td>JD {item.UnitPrice}</td>
                </tr>"));

            return $@"
                <div style='font-family:Arial,sans-serif;color:#333;max-width:700px'>
                    <h2 style='color:#15406b'>
                        Order #{order.ShopOrderMasterId} Successfully Confirmed
                    </h2>

                    <p>Hello <b>{order.FirstName} {order.LastName}</b>,</p>
                    <p>Thank you for choosing us. Your order is being prepared with care.</p>

                    <h3 style='color:#15406b'>Order Summary</h3>
                    <table width='100%' cellpadding='8' cellspacing='0' border='1' style='border-collapse:collapse'>
                        <thead style='background:#f4f7ff'>
                            <tr>
                                <th>Product</th>
                                <th>Qty</th>
                                <th>Price</th>
                            </tr>
                        </thead>
                        <tbody>
                            {itemsHtml}
                        </tbody>
                    </table>

                    <p><b>Subtotal:</b> JD {order.ShopOrderMaster?.SubTotal}<br/>
                       <b>Shipping:</b> JD {order.ShopOrderMaster?.ShippingCost}<br/>
                       <b>Total:</b> JD {order.ShopOrderMaster?.TotalPrice}</p>

                    <h3 style='color:#15406b'>Shipping Address</h3>
                    <p>{order.FirstName} {order.LastName}<br/>
                       {order.Address}<br/>
                       {order.City}, {order.Country}</p>

                    <h3 style='color:#15406b'>Payment Method</h3>
                    <p>{order.PaymentMethod?.PaymentMethodName}</p>

                    <p style='margin-top:30px'>Thank you for trusting <b>PetJoy</b>🐾</p>
                </div>";
        }

        // Build Order Received order Email
        public string BuildOrderReceivedEmail(CheckOut order)
        {
            var itemsHtml = string.Join("", order.OrderItems.Select(item => $@"
        <tr>
            <td>{item.ShopProduct?.Name}</td>
            <td>{item.Quantity}</td>
            <td>JD {item.UnitPrice}</td>
        </tr>"));

            return $@"
    <div style='font-family:Arial,sans-serif;color:#333;max-width:700px'>
        <h2 style='color:#1f3c88'>
             Order #{order.ShopOrderMasterId} Received
        </h2>

        <p>Hello <b>{order.FirstName} {order.LastName}</b>,</p>

        <p>
            We’re happy to inform you that your order has been 
            <b>Received</b>
        </p>

        <h3 style='color:#1f3c88'>Order Details</h3>
        <table width='100%' cellpadding='8' cellspacing='0' border='1' style='border-collapse:collapse'>
            <thead style='background:#eef3ff'>
                <tr>
                    <th align='left'>Product</th>
                    <th>Qty</th>
                    <th>Price</th>
                </tr>
            </thead>
            <tbody>
                {itemsHtml}
            </tbody>
        </table>

        <p style='margin-top:15px'>
            <b>Subtotal:</b> JD {order.ShopOrderMaster?.SubTotal}<br/>
            <b>Shipping:</b> JD {order.ShopOrderMaster?.ShippingCost}<br/>
            <b>Total:</b> JD {order.ShopOrderMaster?.TotalPrice}
        </p>

        <h3 style='color:#1f3c88'>Delivery Address</h3>
        <p>
            {order.FirstName} {order.LastName}<br/>
            {order.Address}<br/>
            {order.City}, {order.Country}
        </p>

        <p style='margin-top:30px'>
            Thank you for shopping with <b>PetJoy</b> 🐾<br/>
            We’ll notify you once your order is out for delivery.
        </p>
    </div>";
        }



        // Order Cancelled Email
        public string BuildOrderCancelledEmail(CheckOut order)
        {
            var itemsHtml = string.Join("", order.OrderItems.Select(item => $@"
        <tr>
            <td>{item.ShopProduct?.Name}</td>
            <td>{item.Quantity}</td>
            <td>JD {item.UnitPrice}</td>
        </tr>
    "));

                    return $@"
        <div style='font-family:Arial,sans-serif;color:#333;max-width:700px'>
            <h2 style='color:#1f3c88'>
                Order #{order.ShopOrderMasterId} Cancelled
            </h2>

            <p>Hello <b>{order.FirstName} {order.LastName}</b>,</p>

            <p>
                We regret to inform you that your order has been 
                <b>Cancelled</b>.
            </p>

            <h3 style='color:#1f3c88'>Order Details</h3>
            <table width='100%' cellpadding='8' cellspacing='0' border='1' style='border-collapse:collapse'>
                <thead style='background:#eef3ff'>
                    <tr>
                        <th align='left'>Product</th>
                        <th>Qty</th>
                        <th>Price</th>
                    </tr>
                </thead>
                <tbody>
                    {itemsHtml}
                </tbody>
            </table>

            <p style='margin-top:15px'>
                <b>Subtotal:</b> JD {order.ShopOrderMaster?.SubTotal}<br/>
                <b>Shipping:</b> JD {order.ShopOrderMaster?.ShippingCost}<br/>
                <b>Total:</b> JD {order.ShopOrderMaster?.TotalPrice}
            </p>

            <h3 style='color:#1f3c88'>Delivery Address</h3>
            <p>
                {order.FirstName} {order.LastName}<br/>
                {order.Address}<br/>
                {order.City}, {order.Country}
            </p>

            <p style='margin-top:30px'>
                You are always welcome to place a new order with <b>PetJoy,</b> <br/>
                We’re here whenever you need us.
            </p>

         </div>";


        }



    }
}


            
