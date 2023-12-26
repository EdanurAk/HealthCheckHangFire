namespace GenelProje.Service
{

    public interface IServiceManagement
    {
        public void PeriodicTask();
        public void SendMessage(string customerId);

        //public  void SendCoupon();
        public Task SendCoupon();

    }

}