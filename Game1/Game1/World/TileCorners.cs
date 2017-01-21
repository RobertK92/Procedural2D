using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    public class TileCorners
    {
        public readonly TileId Middle;
        public readonly TileId Top;
        public readonly TileId Bottom;
        public readonly TileId Right;
        public readonly TileId Left;

        public readonly TileId BottomRightEnclave;
        public readonly TileId TopLeftEnclave;
        public readonly TileId BottomLeftEnclave;
        public readonly TileId TopRightEnclave;

        public readonly TileId BottomRightCorner;
        public readonly TileId BottomLeftCorner;
        public readonly TileId TopRightCorner;
        public readonly TileId TopLeftCorner;
                
        public TileCorners(
            TileId middle, 
            TileId top, 
            TileId bottom, 
            TileId right, 
            TileId left, 
            TileId bottomRightEnclave, 
            TileId topLeftEnclave,
            TileId bottomLeftEnclave,
            TileId topRightEnclave,
            TileId bottomRightCorner,
            TileId bottomLeftCorner,
            TileId topRightCorner,
            TileId topLeftCorner)
        {

            this.TopRightEnclave = topRightEnclave;
            this.BottomLeftEnclave = bottomLeftEnclave;
            this.TopLeftEnclave = topLeftEnclave;
            this.BottomRightEnclave = bottomRightEnclave;

            this.BottomRightCorner = bottomRightCorner;
            this.BottomLeftCorner = bottomLeftCorner;
            this.TopRightCorner = topRightCorner;
            this.TopLeftCorner = topLeftCorner;

            this.Middle = middle;
            this.Top = top;
            this.Bottom = bottom;
            this.Right = right;
            this.Left = left;
        }
    }
}
