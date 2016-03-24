namespace HTMLEngine.Core
{
    internal class DeviceChunkDrawTextEffect : DeviceChunkDrawText
    {
        public DrawTextEffect Effect;
        public HtColor EffectColor;
        public int EffectAmount;

        public override void Draw(float deltaTime)
        {
            bool isTextEmpty = this.Text.Length == 1 && this.Text[0] <= ' ';
            switch (this.Effect)
            {
                case DrawTextEffect.Shadow:
                    if (!isTextEmpty)
                    {
                        this.Font.Draw(this.Rect.Offset(this.EffectAmount, this.EffectAmount), this.EffectColor,
                                       this.Text);
                    }
                    break;
                case DrawTextEffect.Outline:
                    if (!isTextEmpty)
                    {
                        this.Font.Draw(this.Rect.Offset(this.EffectAmount, 0), this.EffectColor, this.Text);
                        this.Font.Draw(this.Rect.Offset(-this.EffectAmount, 0), this.EffectColor, this.Text);
                        this.Font.Draw(this.Rect.Offset(0, this.EffectAmount), this.EffectColor, this.Text);
                        this.Font.Draw(this.Rect.Offset(0, -this.EffectAmount), this.EffectColor, this.Text);
                    }
                    break;
            }
            // draw my text
            base.Draw(deltaTime);
        }
    }
}