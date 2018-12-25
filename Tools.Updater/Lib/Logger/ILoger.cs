using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AutoUpdater.Loger
{
    public interface ILoger
    {
        void Info(string info);

        void Error(string err);

        void Debug(params object[] objects);

        void Warning(string warning);
    }
}
