
using System.Net.Mail;
using System.Net;

namespace EduSpaceEngine.Services.Email
{
    public class EmailService : IEmailService
    {
        public async Task SendChangeEmailCodeAsync(string email, string user, int randomNumber)
        {
            string messageBody = $@"

            <!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
            <html xmlns=""http://www.w3.org/1999/xhtml"">

            <head>
              <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
              <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
              <title>Verify your account</title>

              <style>
                .button {{
                        display: inline-block;
                        background-color: #007bff;
                        color: white !important;
                        border: none;
                        border-radius: 20px;
                        padding: 10px 20px;
                        text-decoration: none;
                        cursor: pointer;
                    }}
              </style>
            </head>


            <body style=""font-family: Helvetica, Arial, sans-serif; margin: 0px; padding: 0px; background-color: #ffffff;"">
              <table role=""presentation""
                style=""width: 100%; border-collapse: collapse; border: 0px; border-spacing: 0px; font-family: Arial, Helvetica, sans-serif; background-color: rgb(239, 239, 239);"">
                <tbody>
                  <tr>
                    <td align=""center"" style=""padding: 1rem 2rem; vertical-align: top; width: 100%;"">
                      <table role=""presentation"" style=""max-width: 600px; border-collapse: collapse; border: 0px; border-spacing: 0px; text-align: left;"">
                        <tbody>
                          <tr>
                            <td style=""padding: 40px 0px 0px;"">
                              <div style=""text-align: left;"">
                                <div style=""padding-bottom: 20px;""><img src=""https://firebasestorage.googleapis.com/v0/b/eduspace-a81b5.appspot.com/o/EduSpaceLogo.png?alt=media&token=7b7dc8a5-05d8-4348-9b4c-c19913949c67"" alt=""Company"" style=""width: 56px;""></div>
                              </div>

                              <div style=""padding: 20px; background-color: rgb(255, 255, 255); border-radius: 20px;"">
                              <div style=""color: rgb(0, 0, 0); text-align: center;"">
                                <h1 style=""margin: 1rem 0"">👌</h1>
                                <h1 style=""margin: 1rem 0"">მოგესალმებით, {user} !</h1>
                                <p style=""padding-bottom: 16px"">გმადლობთ EduSpace-ზე თქვენი ელ.ფოსტის მისამართის განახლებისთვის. თქვენი ახალი ელფოსტის დასადასტურებლად, გთხოვთ, შეიყვანოთ შემდეგი კოდი:</p>
                                <div  class='button'>{randomNumber}</div>
                                <p style=""padding-bottom: 16px"">თუ თქვენ არ მოითხოვეთ ეს ცვლილება, შეგიძლიათ უგულებელყოთ ეს ელფოსტა</p>
                                <p style=""padding-bottom: 16px"">Thank you, EduSpace Team</p>
                              </div>
                            </div>
                            <div style=""padding-top: 20px; color: rgb(153, 153, 153); text-align: center;"">
                              <p style=""padding-bottom: 16px"">© 2023 Nikoloza. All rights reserved.</p>
                            </div>

                            </td>
                          </tr>
                        </tbody>
                      </table>
                    </td>
                  </tr>
                </tbody>
              </table>
            </body>

            </html>";

            using (MailMessage message = new MailMessage("noreplynika@gmail.com", email))

            {

                message.Subject = "EduSpace.ge ანგარიშის აღდგენა";

                message.Body = messageBody;

                message.IsBodyHtml = true;

                using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))

                {

                    smtpClient.Credentials = new NetworkCredential("noreplynika@gmail.com", "cdqwvhmdwljietwq");
                    smtpClient.EnableSsl = true;

                    try

                    {
                        await smtpClient.SendMailAsync(message);
                    }

                    catch (Exception)

                    {
                        // Handle any exception that occurs during the email sending process
                        // You can log the error or perform other error handling actions
                    }

                }

            }
        }

        public async Task SendEmailAsync(string email, string user, string confirmationLink)
        {
            string messageBody = $@"
            <!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
            <html xmlns=""http://www.w3.org/1999/xhtml"">

            <head>
              <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
              <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
              <title>Verify your account</title>

              <style>
                .button {{
                        display: inline-block;
                        background-color: #007bff;
                        color: white !important;
                        border: none;
                        border-radius: 20px;
                        padding: 10px 20px;
                        text-decoration: none;
                        cursor: pointer;
                    }}
              </style>
            </head>


            <body style=""font-family: Helvetica, Arial, sans-serif; margin: 0px; padding: 0px; background-color: #ffffff;"">
              <table role=""presentation""
                style=""width: 100%; border-collapse: collapse; border: 0px; border-spacing: 0px; font-family: Arial, Helvetica, sans-serif; background-color: rgb(239, 239, 239);"">
                <tbody>
                  <tr>
                    <td align=""center"" style=""padding: 1rem 2rem; vertical-align: top; width: 100%;"">
                      <table role=""presentation"" style=""max-width: 600px; border-collapse: collapse; border: 0px; border-spacing: 0px; text-align: left;"">
                        <tbody>
                          <tr>
                            <td style=""padding: 40px 0px 0px;"">
                              <div style=""text-align: left;"">
                                <div style=""padding-bottom: 20px;""><img src=""https://firebasestorage.googleapis.com/v0/b/eduspace-a81b5.appspot.com/o/EduSpaceLogo.png?alt=media&token=7b7dc8a5-05d8-4348-9b4c-c19913949c67"" alt=""Company"" style=""width: 56px;""></div>
                              </div>
                              <div style=""padding: 20px; background-color: rgb(255, 255, 255); border-radius: 20px;"">
                                <div style=""color: rgb(0, 0, 0); text-align: center;"">
                                  <h1 style=""margin: 1rem 0"">🔒</h1>
                                  <h1 style=""margin: 1rem 0"">მოგესალმებით, {user}</h1>
                                  <p style=""padding-bottom: 16px"">თქვენი EduSpace-ს ანგარიშიდან მოთხოვნილია პაროლის აღდგენა. ახალი პაროლის დასაყენებლად გთხოვთ დააჭიროთ პაროლის აღდგენის ღილაკს.</p>
                                  <a href={confirmationLink} class='button'>პაროლის აღდგენა</a>
                                  <p style=""padding-bottom: 16px"">თუ პაროლის გადაყენება არ მოგითხოვიათ, შეგიძლიათ უგულებელყოთ ეს ელფოსტა.</p>
                                  <p style=""padding-bottom: 16px"">Thank you, EduSpace Team</p>
                                </div>
                              </div>
                              <div style=""padding-top: 20px; color: rgb(153, 153, 153); text-align: center;"">
                                <p style=""padding-bottom: 16px"">© 2023 Nikoloza. ყველა უფლება დაცულია</p>
                              </div>
                            </td>
                          </tr>
                        </tbody>
                      </table>
                    </td>
                  </tr>
                </tbody>
              </table>
            </body>

            </html>";

            using (MailMessage message = new MailMessage("noreplynika@gmail.com", email))
            {
                message.Subject = "EduSpace.ge ანგარიშის აღდგენა";
                message.Body = messageBody;
                message.IsBodyHtml = true;

                using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.Credentials = new NetworkCredential("noreplynika@gmail.com", "cdqwvhmdwljietwq");
                    smtpClient.EnableSsl = true;

                    try
                    {
                        await smtpClient.SendMailAsync(message);
                    }
                    catch (Exception)
                    {
                        // Handle any exception that occurs during the email sending process
                        // You can log the error or perform other error handling actions
                    }
                }
            }
        }

        public async Task SendVerificationEmailAsync(string email, string user, string confirmationLink)
        {
            string messageBody = $@"
            <!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
            <html xmlns=""http://www.w3.org/1999/xhtml"">

            <head>
              <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
              <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
              <title>Verify your account</title>

              <style>
                .button {{
                        display: inline-block;
                        background-color: #007bff;
                        color: white !important;
                        border: none;
                        border-radius: 20px;
                        padding: 10px 20px;
                        text-decoration: none;
                        cursor: pointer;
                    }}
              </style>
            </head>


            <body style=""font-family: Helvetica, Arial, sans-serif; margin: 0px; padding: 0px; background-color: #ffffff;"">
              <table role=""presentation""
                style=""width: 100%; border-collapse: collapse; border: 0px; border-spacing: 0px; font-family: Arial, Helvetica, sans-serif; background-color: rgb(239, 239, 239);"">
                <tbody>
                  <tr>
                    <td align=""center"" style=""padding: 1rem 2rem; vertical-align: top; width: 100%;"">
                      <table role=""presentation"" style=""max-width: 600px; border-collapse: collapse; border: 0px; border-spacing: 0px; text-align: left;"">
                        <tbody>
                          <tr>
                            <td style=""padding: 40px 0px 0px;"">
                              <div style=""text-align: left;"">
                                <div style=""padding-bottom: 20px;""><img src=""https://firebasestorage.googleapis.com/v0/b/eduspace-a81b5.appspot.com/o/EduSpaceLogo.png?alt=media&token=7b7dc8a5-05d8-4348-9b4c-c19913949c67"" alt=""Company"" style=""width: 56px;""></div>
                              </div>
                              <div style=""padding: 20px; background-color: rgb(255, 255, 255); border-radius: 20px;"">
                                <div style=""color: rgb(0, 0, 0); text-align: center;"">
                                  <h1 style=""margin: 1rem 0"">👋</h1>
                                  <h1 style=""margin: 1rem 0"">მოგესალმებით, {user} !</h1>
                                  <p style=""padding-bottom: 16px"">გმადლობთ, რომ დარეგისტრირდით EduSpace-ზე თქვენი ანგარიშის გასააქტიურებლად, გთხოვთ,დააჭიროთ ქვემოთ მოცემულ ღილაკს</p>
                                  <a href={confirmationLink} class='button'>გააქტიურება</a>
                                  <p style=""padding-bottom: 16px"">თუ ამ მისამართის დადასტურება არ მოგითხოვიათ, შეგიძლიათ იგნორირება გაუკეთოთ ამ ელფოსტას.</p>
                                  <p style=""padding-bottom: 16px"">Thank you, EduSpace Team</p>
                                </div>
                              </div>
                              <div style=""padding-top: 20px; color: rgb(153, 153, 153); text-align: center;"">
                                <p style=""padding-bottom: 16px"">© 2023 Nikoloza. ყველა უფლება დაცულია</p>
                              </div>
                            </td>
                          </tr>
                        </tbody>
                      </table>
                    </td>
                  </tr>
                </tbody>
              </table>
            </body>

            </html>";

            using (MailMessage message = new MailMessage("noreplynika@gmail.com", email))
            {
                message.Subject = "EduSpace.ge მომხმარებლის აქტივაცია";
                message.Body = messageBody;
                message.IsBodyHtml = true;

                using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.Credentials = new NetworkCredential("noreplynika@gmail.com", "cdqwvhmdwljietwq");
                    smtpClient.EnableSsl = true;

                    try
                    {
                        await smtpClient.SendMailAsync(message);
                    }
                    catch (Exception)
                    {
                        // Handle any exception that occurs during the email sending process
                        // You can log the error or perform other error handling actions
                    }
                }
            }
        }

        public async Task SendWarningEmailAsync(string email, string user)
        {
            string messageBody = $@"

            <!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
            <html xmlns=""http://www.w3.org/1999/xhtml"">

            <head>
              <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
              <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
              <title>Verify your account</title>

              <style>

                .button {{
                        display: inline-block;
                        background-color: #007bff;
                        color: white !important;
                        border: none;
                        border-radius: 20px;
                        padding: 10px 20px;
                        text-decoration: none;
                        cursor: pointer;

                    }}
              </style>
            </head>

            <body style=""font-family: Helvetica, Arial, sans-serif; margin: 0px; padding: 0px; background-color: #ffffff;"">
              <table role=""presentation""
                style=""width: 100%; border-collapse: collapse; border: 0px; border-spacing: 0px; font-family: Arial, Helvetica, sans-serif; background-color: rgb(239, 239, 239);"">
                <tbody>
                  <tr>
                    <td align=""center"" style=""padding: 1rem 2rem; vertical-align: top; width: 100%;"">
                      <table role=""presentation"" style=""max-width: 600px; border-collapse: collapse; border: 0px; border-spacing: 0px; text-align: left;"">
                        <tbody>
                          <tr>
                            <td style=""padding: 40px 0px 0px;"">
                              <div style=""text-align: left;"">
                                <div style=""padding-bottom: 20px;""><img src=""https://firebasestorage.googleapis.com/v0/b/eduspace-a81b5.appspot.com/o/EduSpaceLogo.png?alt=media&token=7b7dc8a5-05d8-4348-9b4c-c19913949c67"" alt=""Company"" style=""width: 56px;""></div>
                              </div>
                             <div style=""padding: 20px; background-color: rgb(255, 255, 255); border-radius: 20px;"">
                              <div style=""color: rgb(0, 0, 0); text-align: center;"">
                                <h1 style=""margin: 1rem 0"">⚠️</h1>
                                <h1 style=""margin: 1rem 0"">მოგესალმებით, {user} !</h1>
                                <p style=""padding-bottom: 16px"">ჩვენ შევამჩნიეთ, რომ თქვენ მოითხოვეთ ელფოსტის მისამართის შეცვლა, რომელიც დაკავშირებულია თქვენს EduSpace ანგარიშთან.</p>
                                <p style=""padding-bottom: 16px"">თუ თქვენ არ მოითხოვეთ ეს ცვლილება, შეგიძლიათ უგულებელყოთ ეს ელფოსტა.</p>
                                <p style=""padding-bottom: 16px"">Thank you, EduSpace Team</p>
                              </div>
                            </div>
                            <div style=""padding-top: 20px; color: rgb(153, 153, 153); text-align: center;"">
                              <p style=""padding-bottom: 16px"">© 2023 Nikoloza. All rights reserved.</p>
                            </div>
                            </td>
                          </tr>
                        </tbody>
                      </table>
                    </td>
                  </tr>
                </tbody>
              </table>
            </body>
            </html>";


            using (MailMessage message = new MailMessage("noreplynika@gmail.com", email))

            {
                message.Subject = "EduSpace.ge მომხმარებლის აქტივაცია";
                message.Body = messageBody;
                message.IsBodyHtml = true;

                using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587))

                {

                    smtpClient.Credentials = new NetworkCredential("noreplynika@gmail.com", "cdqwvhmdwljietwq");
                    smtpClient.EnableSsl = true;

                    try
                    {
                        await smtpClient.SendMailAsync(message);
                    }
                    catch (Exception)
                    {
                        // Handle any exception that occurs during the email sending process
                        // You can log the error or perform other error handling actions
                    }
                }
            }
        }
    }
}
