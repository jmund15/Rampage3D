using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public interface ISpriteComponent
{
    public float GetSpriteHeight();
    public float GetSpriteWidth();
    public bool FlipH { get; set; }
    public bool FlipV { get; set; }
    public Vector2 Offset { get; set; }
    //public void SetFlipH(bool flip);
    //public void SetFlipV(bool flip);
    //public bool GetFlipH();
    //public bool GetFlipV();
    
    // Even for mult sprites, text should be the same???
    public Texture2D GetTexture();

    public void Hide();
    public void Show();
    public Node GetInterfaceNode();
    //public void SetTexture(Texture2D texture);
}
