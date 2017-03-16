using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;
using System.Linq;

class Vihu : PhysicsObject
{
    private DoubleMeter elamaLaskuri = new DoubleMeter(3, 0, 10);
    public DoubleMeter Elamalaskuri { get { return elamaLaskuri; } }

    public Vihu(double leveys, double korkeus)
        : base(leveys, korkeus)
    {
        elamaLaskuri.LowerLimit += delegate { this.Destroy(); };
    }


    public void MuutaElamamaara (double uusimaara)
    {
        this.elamaLaskuri.Value = uusimaara;
    }
}