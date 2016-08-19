namespace GraphTest.BankModel.Model
{
    class Customer
    {
        /// <summary>
        /// Structure: "c0".
        /// As different types of nodes to be hold in a network
        /// </summary>
        internal string ID;

        internal Customer(int id)
        {
            ID = "c"+id;
        }
    }
}
