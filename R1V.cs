namespace QuarterMaster
{
    public class R1V
    {
        private int _count;
        private readonly string _value;

        public R1V(string value)
        {
            _count = 1;
            _value = value;
        }

        public string Get()
        {
            if (_count == 1)
            {
                _count = 0;
                return _value;
            }
            else
            {
                return null;
            }
        }
    }
}
