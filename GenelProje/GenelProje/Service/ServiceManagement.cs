using GenelProje.Models;

namespace GenelProje.Service
{
    public class ServiceManagement : IServiceManagement
    {
        private readonly NorthwndContext _northwndContext;

        public ServiceManagement(NorthwndContext northwndContext)
        {
            _northwndContext = northwndContext;
        }

        public void PeriodicTask()
        {

            //Hangfire tarafından belirli aralıklarla çalıştırılacak olan işlemler

            // güncelleme işlemi
            var recordsToUpdate = _northwndContext.Products.ToList();

            foreach (var record in recordsToUpdate)
            {
                record.UnitPrice++;
            }

            Console.WriteLine("deneme" + DateTime.Now);
            _northwndContext.SaveChanges();

        }
        public async Task SendCoupon()
        {

            Console.WriteLine("Coupon sent!");

            await Task.CompletedTask;
        }

        //public void SendCoupon()
        //{
        //    Console.WriteLine("Size özel kupon tanımlandı kullanabilirsiniz");
        //}

        public void SendMessage(string customerId)
        {
            Console.WriteLine($"Dear {customerId}, you have been registered. Welcome!");
        }
    
    

    }
}
