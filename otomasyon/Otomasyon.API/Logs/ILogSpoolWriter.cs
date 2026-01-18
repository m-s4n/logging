using Otomasyon.Domain.Entities;

namespace Otomasyon.API.Logs;

public interface ILogSpoolWriter
{
    void Write(RequestLog log);
    void Write(ErrorLog log);
}