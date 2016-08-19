namespace GraphTest.BankModel.Classes
{
    sealed internal class EventInt // todo refactor(?) eventIterator
    {
        //public delegate void MethodContainer(T item);
        //public event MethodContainer OnAdd;
        //public event MethodContainer OnRemove;

        internal delegate void ValueChangedEventHandler();

        public event ValueChangedEventHandler Incremented;

        private int _value;

        public EventInt(int value)
        {
            _value = value;
        }

        /// <summary>
        /// Increments the private value
        /// and call the corresponding event
        /// </summary>
        public void Plus()
        {
            _value++;
            Incremented();
        }

        public int GetValue()
        {
            return _value;
        }
        public int ToInt()
        {
            return _value;
        }

        public void SetValue(int value)
        {
            _value = value;
        }
        

        //private T _value;
        /*
        public EventValue(T value)
        {
            _value = value;
        }
         */

        
        /*
        static public operator implicit
        private DWORD(int value)
        {
            return new DWORD(value);
        }

        static public operator implicit int(DWORD value)
        {
            return value.Value;
        }
         * 
         */
    }
}
