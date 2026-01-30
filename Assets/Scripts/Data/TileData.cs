using System;

namespace Match3.Data
{
    /// <summary>
    /// Pure data representation of a tile. No MonoBehaviour.
    /// This is the "source of truth" for the game logic.
    /// </summary>
    [Serializable]
    public class TileData
    {
        public TileType Type;
        public TileType UnderlyingColor;  // For special tiles that retain color identity
        public int X;
        public int Y;
        public bool IsMatched;
        public bool IsNew;  // Just spawned, needs drop animation
        
        // Ice overlay (0 = no ice, 1-3 = ice layers)
        public int IceLevel;
        
        public bool HasIce => IceLevel > 0;
        
        public TileData(TileType type, int x, int y)
        {
            Type = type;
            UnderlyingColor = type.IsNormalGem() ? type : TileType.None;
            X = x;
            Y = y;
            IsMatched = false;
            IsNew = false;
            IceLevel = 0;
        }
        
        public TileData Clone()
        {
            return new TileData(Type, X, Y)
            {
                UnderlyingColor = this.UnderlyingColor,
                IsMatched = this.IsMatched,
                IsNew = this.IsNew,
                IceLevel = this.IceLevel
            };
        }
        
        public void SetPosition(int x, int y)
        {
            X = x;
            Y = y;
        }
        
        public bool CanMatch()
        {
            return Type.IsMatchable() && !IsMatched;
        }
        
        public bool MatchesWith(TileData other)
        {
            if (other == null) return false;
            if (!CanMatch() || !other.CanMatch()) return false;
            
            // Rainbow matches with anything
            if (Type == TileType.Rainbow || other.Type == TileType.Rainbow)
                return true;
            
            // Normal gem matching
            if (Type.IsNormalGem() && other.Type.IsNormalGem())
                return Type == other.Type;
            
            // Special tile with underlying color
            var myColor = UnderlyingColor != TileType.None ? UnderlyingColor : Type;
            var otherColor = other.UnderlyingColor != TileType.None ? other.UnderlyingColor : other.Type;
            
            return myColor == otherColor;
        }
        
        public override string ToString()
        {
            return $"Tile({Type} at {X},{Y})";
        }
    }
}
