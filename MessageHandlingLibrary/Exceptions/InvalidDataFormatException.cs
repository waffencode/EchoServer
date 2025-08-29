using System;

namespace MessageHandlingLibrary.Exceptions
{
    public class InvalidDataFormatException : Exception
    {
        public InvalidDataFormatException() : base("Ошибка формата: неверный формат данных") 
        { 

        }
    }
}
