using System;

namespace MessageHandlingLibrary.Exceptions
{
    public class LengthExceededException : Exception
    {
        public LengthExceededException() : base("Ошибка: сообщение превысило максимальную длину")
        {

        }
    }
}
