namespace MusiBotProd.Clients
{
    public interface IClient
    {
        public bool Login();
        public void Execute(); 
        public void AddModules();
    }
}
