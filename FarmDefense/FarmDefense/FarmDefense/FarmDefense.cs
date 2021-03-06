﻿using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;
using System.Linq;

public class FarmDefense : PhysicsGame
{
    Image nurmikonKuva = LoadImage("nurmikko");
    Image tienKuva = LoadImage("tie");
    Image kanankuva = LoadImage("kana");
    Image kananmuna = LoadImage("muna1");
    Image[] Sudenkuvat = LoadImages("sus1", "sus2");

    List<Vector> pisteet;
    Vector[] pisteTaulu;
    List<Vector> rahat;
    Vector[] rahaTaulu;

    int aalto = 1;
    IntMeter elämät;
    IntMeter rahaLaskuri;

    GameObject cursor;

    public override void Begin()
    {
        IsMouseVisible = true;
        SmoothTextures = false;
        Aloitapeli();
    }

    void Aloitapeli()
    {
        ClearAll();
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");

        pisteet = new List<Vector>();
        pisteTaulu = new Vector[10];

        elämät = new IntMeter(10, 0, 10);

        Label pisteNaytto = new Label();
        pisteNaytto.X = Screen.Left + 100;
        pisteNaytto.Y = Screen.Top - 100;
        pisteNaytto.TextColor = Color.Black;
        pisteNaytto.Color = Color.White;
        pisteNaytto.BindTo(elämät);
        Add(pisteNaytto);

        elämät.LowerLimit += GameOver;


        rahaLaskuri = new IntMeter(20, 0, 9999 );

        Label rahaNaytto = new Label();
        rahaNaytto.X = Screen.Right - 100;
        rahaNaytto.Y = Screen.Top - 100;
        rahaNaytto.TextColor = Color.White;
        rahaNaytto.Color = Color.Blue;
        rahaNaytto.Title = "Rahat";
        rahaNaytto.BindTo(rahaLaskuri);
        Add(rahaNaytto);


        TileMap ruudut = TileMap.FromLevelAsset("kenttä1");

        ruudut.SetTileMethod('.', LuoNurmikko);
        ruudut.SetTileMethod('T', LuoTie);
        ruudut.SetTileMethod('0', LuoKulma, 0);
        ruudut.SetTileMethod('1', LuoKulma, 1);
        ruudut.SetTileMethod('2', LuoKulma, 2);
        ruudut.SetTileMethod('3', LuoKulma, 3);
        ruudut.SetTileMethod('4', LuoKulma, 4);
        ruudut.SetTileMethod('5', LuoKulma, 5);
        ruudut.SetTileMethod('6', LuoKulma, 6);
        ruudut.SetTileMethod('7', LuoKulma, 7);
        ruudut.SetTileMethod('8', LuoKulma, 8);
        ruudut.SetTileMethod('9', LuoKulma, 9);
        ruudut.Execute(60, 100);

        foreach (Vector p in pisteTaulu)
        {
            if (p != Vector.Zero)
            {
                pisteet.Add(p);
            }
        }

        Timer.SingleShot(2, Luoaalto);

        Luopainikkeet();

        cursor = new GameObject(50, 50);
        cursor.Color = Color.Transparent;
        Add(cursor);
        Mouse.ListenMovement(0, hiirenliikutus, null);
        Mouse.Listen(MouseButton.Left, ButtonState.Released, LuoKana, null);
        Camera.ZoomToLevel();
    }

    void LuoKana()
    {
        if (cursor.Image == null)
        {
            return;
        }

        GameObject kana = new GameObject(50, 50);
        kana.Image = cursor.Image;
        kana.Position = Mouse.PositionOnWorld;
        Add(kana);

        AssaultRifle ase;
        ase = new AssaultRifle(30, 10);
        ase.ProjectileCollision = AmmusOsui;
        ase.IsVisible = false;
        kana.Add(ase);

        Timer ajastin = new Timer();
        ajastin.Interval = 0.8;
        ajastin.Timeout += delegate
        {
            var vihut = GetObjectsWithTag("susi").Where((GameObject o) => Vector.Distance(kana.Position, o.Position) < 300).ToList();
            vihut.Sort((GameObject o1, GameObject o2) => Vector.Distance(o1.Position, kana.Position).CompareTo(Vector.Distance(o2.Position, kana.Position)));

            if (vihut.Count > 0)
            {
                ase.AbsoluteAngle = (vihut[0].Position - kana.Position).Angle;
                AmmuAseella(ase);
            }
        };
        ajastin.Start();

        cursor.Image = null;
    }

    void AmmusOsui(PhysicsObject ammus, PhysicsObject kohde)
    {
        ammus.Destroy();
        if (kohde.Tag == "susi")
        {
            Vihu vihu = (Vihu)kohde;
            vihu.Elamalaskuri.Value--;
            if (vihu.Elamalaskuri.Value == vihu.Elamalaskuri.MinValue) rahaLaskuri.Value += 1;
        }
    }

    void AmmuAseella(AssaultRifle ase)
    {
        PhysicsObject ammus = ase.Shoot();

        if (ammus != null)
        {
            ammus.Size *= 1.5;
            ammus.Image = kananmuna;
            ammus.MaximumLifetime = TimeSpan.FromSeconds(8.0);
        }
    }

    void hiirenliikutus(AnalogState hiirenTila)
    {
        cursor.X = Mouse.PositionOnWorld.X;
        cursor.Y = Mouse.PositionOnWorld.Y;
    }


    void Luopainikkeet()
    {
        GameObject kana = new GameObject(50, 50);
        kana.Image = kanankuva;
        kana.Position = new Vector(Level.Right - 50, 0);
        Add(kana);
        Mouse.ListenOn(kana, MouseButton.Left, ButtonState.Pressed, delegate
        {
            if (rahaLaskuri.Value >= 10)
            {
                rahaLaskuri.Value -= 10;
                cursor.Image = kana.Image;
            }

        }, null);

    }

    void GameOver()
    {
        MultiSelectWindow valikko = new MultiSelectWindow("Hävisit!", "Uudestaan", "Lopeta");
        valikko.ItemSelected += PainettiinValikonNappia;
        Add(valikko);
    }

    void PainettiinValikonNappia(int valinta)
    {
        switch (valinta)
        {
            case 0:
                Aloitapeli();
                break;
            case 1:
                Exit();
                break;
        }
    }


    void LuoKentta()
    {
        TileMap ruudut = TileMap.FromLevelAsset("kenttä1");
        ruudut.SetTileMethod('T', LuoTie);
        ruudut.SetTileMethod('.', LuoNurmikko);
        ruudut.Execute(20, 20);
    }
    void LuoNurmikko(Vector paikka, double leveys, double korkeus)
    {
        GameObject nurmikko = new GameObject(leveys, korkeus);
        nurmikko.Position = paikka;
        nurmikko.Shape = Shape.Rectangle;
        nurmikko.Tag = "nurmikko";
        nurmikko.Image = nurmikonKuva;
        Add(nurmikko, -1);
    }

    void LuoTie(Vector paikka, double leveys, double korkeus)
    {
        GameObject tie = new GameObject(leveys, korkeus);
        tie.Position = paikka;
        tie.Shape = Shape.Rectangle;
        tie.Tag = "tie";
        tie.Image = tienKuva;
        Add(tie, -1);
    }

    void LuoKulma(Vector paikka, double leveys, double korkeus, int numero)
    {
        GameObject tie = new GameObject(leveys, korkeus);
        tie.Position = paikka;
        tie.Shape = Shape.Rectangle;
        tie.Tag = "tie";
        tie.Image = tienKuva;
        Add(tie, -1);

        pisteTaulu[numero] = paikka;
    }

    void Luoaalto()
    {
        int susia = 5 + aalto * 3;

        Timer ajastin = new Timer();
        ajastin.Interval = 1.5;
        ajastin.Timeout += delegate
        {
            if (susia > 0)
            {
                susia--;
                MessageDisplay.Add("Uusi susi energiamäärällä: " + 3 * (aalto / 2.0));
                LuoSusi(3 * (aalto / 2.0 ));
            }
            else
            {
                ajastin.Stop();
                MessageDisplay.Add("Aalto loppui, seuraava 10s kuluttua!");
                aalto++;
                Timer.SingleShot(10, Luoaalto);
            }
        };
        ajastin.Start();

    }

    void LuoSusi(double kestavyys)
    {
        Vihu susi = new Vihu(100, 100);
        susi.Image = Sudenkuvat[0];
        Animation animaatio = new Animation(Sudenkuvat);
        animaatio.FPS = 4;
        susi.Animation = animaatio;
        susi.Position = pisteet[0];
        susi.Animation.Start();
        susi.CollisionIgnoreGroup = 1;
        susi.CanRotate = false;
        susi.Tag = "susi";
        susi.MuutaElamamaara(kestavyys);
        Add(susi);

        Timer sudenkaanto = new Timer();
        sudenkaanto.Interval = 0.1;
        sudenkaanto.Timeout += delegate { KaannaSusi(susi); };
        sudenkaanto.Start();


        PathFollowerBrain Aivo = new PathFollowerBrain();
        Aivo.Path = pisteet;
        Aivo.Speed = 100;
        Aivo.ArrivedAtEnd += delegate
        {
         
            elämät.Value--;
            susi.Destroy();
        };
        susi.Brain = Aivo;
    }
    void KaannaSusi(Vihu susi)
    {
        susi.Angle = susi.Velocity.Angle;
    }


}

