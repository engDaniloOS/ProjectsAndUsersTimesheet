using System;

namespace TimeSheet.DataProviders.Repository
{
    /// <summary>
    /// Lançada pelo repositório quando o banco rejeita uma gravação por violar um índice único
    /// (ex.: login ou e-mail duplicado). Isola o serviço do provedor de banco de dados.
    /// </summary>
    public class UniqueConstraintViolationException : Exception
    {
        public UniqueConstraintViolationException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
