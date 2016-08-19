namespace GraphTest.BankModel.Enums
{
    class Policies
    {
        internal enum GraphType
        {
            BarabasiAlbertGraph,
            RandomPowerlawTree,
            PowerlawClusterGraph,
            ScaleFree,
            ConnectedWattsStrogatzGraph,
            ErdosRenyi
        }
    }
}
