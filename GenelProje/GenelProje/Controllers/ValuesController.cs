using GenelProje.Models;
using GenelProje.Service;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GenelProje.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        public readonly ILogger _logger;
        private readonly NorthwndContext _context;
        public ValuesController(ILogger<ValuesController> logger, NorthwndContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost("FireAndForget")]
        public IActionResult AddCustomer(Customer customer)
        {
            if (customer.CustomerId != null && customer.Address != null && customer.ContactName != null)
            {
                _context.Add(customer);
                _context.SaveChanges();

                BackgroundJob.Enqueue<IServiceManagement>(x => x.SendMessage(customer.CustomerId));

                _logger.LogInformation("FireAndForget ~ Veriler Eklendi {@customer} ", customer);
            }
            else
            {
                _logger.LogError("Gerekli alanları boş geçmeyiniz");
            }
            return Ok();
        }

        [HttpPost("Delayed")]
        public IActionResult AddCustomer2(Customer customer)
        {
            _context.Add(customer);
            _context.SaveChanges();


            BackgroundJob.Schedule<IServiceManagement>(x => x.SendCoupon(), TimeSpan.FromMinutes(5));//Schedule dönüş türü task TimeSpan.FromMinutes(5) yaknızca bir kere çalışacak görevlerde kullanılıyor.
            _logger.LogInformation("Delayed ~ Veriler Eklendi {@customer}", customer);
            //BackgroundJob.Schedule<IServiceManagement>(x => x.SendCoupon(), Cron.MinuteInterval(5));

            return Ok();
        }

        [HttpPost("Continue")]//BackgroundJob.ContinueJobWith metodu kullanılarak, ikinci iş birinci iş tamamlandıktan sonra çalışır. 
        public IActionResult Confirm()
        {
            int timeInSeconds = 30;

            var parentJobId = BackgroundJob.Schedule(() => Console.WriteLine("birinci iş!" + DateTime.Now), TimeSpan.FromSeconds(timeInSeconds));
            BackgroundJob.ContinueJobWith(parentJobId, () => Console.WriteLine("ikinci iş!"));

            _logger.LogInformation("Continues Jobs ~ Zincirleme İş Çalıştı");

            return Ok("Confirmation job created!");
        }

        [HttpPost("StartPeriodic")]
        public IActionResult StartPeriodicTask()
        {

            RecurringJob.AddOrUpdate<IServiceManagement>(x => x.PeriodicTask(), Cron.MinuteInterval(1));

            _logger.LogInformation("Periodic Jobs ~ Periyodik İşler Çalıştı");

            //RecurringJob.AddOrUpdate("periodic-task", () => PeriodicTask(), Cron.MinuteInterval(1)); görev ismi çalışacak fonksiyon ve periyod aralığı

            return Ok();
        }


        [HttpPost("StopPeriodic")]
        public IActionResult StopPeriodicTask()
        {
            RecurringJob.RemoveIfExists("IServiceManagement.PeriodicTask");// belirtilen isimde bir görev varsa onu durdurur.

            _logger.LogInformation("Periyodik Çalışan iş durduruldu");
            

            return Ok();
        }

        [HttpPost("Trigger")]
        public IActionResult TriggerTask()
        {

            RecurringJob.Trigger("IServiceManagement.PeriodicTask");//örneğin her hafta çarşamba günü çalışacak görev varsa bunu cuma tetiklersem birdahaki önümüzdeki çarşambaya çalışır.
           
            _logger.LogInformation("Periyodik görev çalışması gereken zamandan önce tetiklendi");

            return Ok();
        }

    }
}
