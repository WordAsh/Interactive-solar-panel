using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace SunLigthRef
{
    public class SunLigthRefInfo : GH_AssemblyInfo
    {
        public override string Name => "SunLigthRef";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("88be187e-0be7-428f-82ea-9448415af433");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}