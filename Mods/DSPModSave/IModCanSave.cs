using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace crecheng.DSPModSave
{
    public interface IModCanSave
    {

        /// <summary>
        /// Save your data
        /// </summary>
        /// <param name="w">Binary Writer</param>
        void Export(BinaryWriter w);

        /// <summary>
        /// Load saved data
        /// </summary>
        /// <param name="r">BinaryReader</param>
        void Import(BinaryReader r);

        /// <summary>
        /// There is no saved data for your mod, initialize from scratch
        /// </summary>
        void IntoOtherSave();
    }
}