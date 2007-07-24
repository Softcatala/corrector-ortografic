using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    /// <summary>
    /// Mètodes per a les classes que mostren tests
    /// </summary>
    public interface IMostraTest
    {
        /// <summary>
        /// Es crida quan s'ha afegit un test.
        /// </summary>
        /// <param name="test">El test que s'ha afegit</param>
        void NouTest(BaseTest test);
    }
}
