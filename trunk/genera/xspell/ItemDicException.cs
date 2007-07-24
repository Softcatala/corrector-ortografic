using System;
using System.Collections.Generic;
using System.Text;

namespace xspell
{
    /// <summary>
    /// Excepcions d'ItemDic
    /// </summary>
    public class ItemDicException : Exception
    {
        public ItemDicException(string missatge)
            : base(missatge)
        {
        }
    }
}
