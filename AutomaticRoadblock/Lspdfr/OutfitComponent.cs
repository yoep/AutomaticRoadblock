using System.Xml.Serialization;

namespace AutomaticRoadblocks.Lspdfr
{
    public class OutfitComponent
    {
        public OutfitComponent()
        {
        }

        public OutfitComponent(int id, int drawable, int texture)
        {
            Id = id;
            Drawable = drawable;
            Texture = texture;
        }

        [XmlAttribute] public int Id { get; internal set; }

        [XmlAttribute] public int Drawable { get; internal set; }

        [XmlAttribute] public int Texture { get; internal set; }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Drawable)}: {Drawable}, {nameof(Texture)}: {Texture}";
        }

        protected bool Equals(OutfitComponent other)
        {
            return Id == other.Id && Drawable == other.Drawable && Texture == other.Texture;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OutfitComponent)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id;
                hashCode = (hashCode * 397) ^ Drawable;
                hashCode = (hashCode * 397) ^ Texture;
                return hashCode;
            }
        }
    }
}