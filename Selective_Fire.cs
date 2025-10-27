// Decompiled with JetBrains decompiler
// Type: SelectiveFire
// Assembly: SelectiveFire, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2F577586-991A-450B-88D3-8132DD6FE939
// Assembly location: D:\SteamLibrary\steamapps\common\Grand Theft Auto V\scripts\SelectiveFire.dll

using GTA;
using GTA.Native;
using NAudio;
using NAudio.Wave;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;


public class SelectiveFire : Script
{
    private int fireMode = 1;
    private int ammo;
    private int ammocount;
    private bool capableWeapon;
    private bool firemodechanged;
    private bool showImg;
    private bool stealth;
    private bool stealthLaunchIfPlayerAiming;
    private bool playerreloaded;
    private bool breathshaking;
    private bool aimshaking;
    private string firemodeImgRoot = AppDomain.CurrentDomain.BaseDirectory + "\\SelectiveFire\\";
    private string firemodeImg;
    private Weapon previousWeapon;
    private Ped player;
    private DateTime imgTimer;
    private ScriptSettings config;
    private Keys ChangeFireModeHotkey_Keys;
    private bool ActivateSelectiveFire;
    private bool ShowImage;
    private bool AutoHideImage;
    private bool StealthIfPlayerAiming;
    private bool StealthAutoDisable;
    private bool WasteAmmo;
    private bool ShowNotif;
    private bool BreathAimMovement;
    private bool ShakeWhenAim;
    private int ShotsPerBurst;
    private int ImageShownTime;
    private int ImageWidth;
    private int ImageHeight;
    private int BreathMovementRate;
    private int ShakeWhenAimMovementRate;

    private int gvd = ScriptSettings.Load("scripts\\SelectiveFire.ini").GetValue<int>("SETTINGS", "GLOBAL_VOLUME_DOWN", 15);
    private WaveFileReader WavereaderDown;
    private WaveChannel32 wavechanDown;
    private DirectSoundOut DSODown;
    private float volumeDown;

    public SelectiveFire()
    {
        this.Tick += new EventHandler(this.OnTick);
        this.KeyUp += new KeyEventHandler(this.OnKeyUp);
        this.Interval = 10;
        this.config = ScriptSettings.Load("scripts\\SelectiveFire.ini");
        string str = this.config.GetValue<string>("HOTKEYS", "ChangeFireMode", "N");
        this.ActivateSelectiveFire = this.config.GetValue<bool>("SELECTIVEFIRE", nameof(SelectiveFire), true);
        this.ShotsPerBurst = this.config.GetValue<int>("SELECTIVEFIRE", nameof(ShotsPerBurst), 3);
        this.ShowImage = this.config.GetValue<bool>("NOTIFICATIONS", nameof(ShowImage), true);
        this.AutoHideImage = this.config.GetValue<bool>("NOTIFICATIONS", nameof(AutoHideImage), true);
        this.ImageShownTime = this.config.GetValue<int>("NOTIFICATIONS", nameof(ImageShownTime), 3);
        this.ShowNotif = this.config.GetValue<bool>("NOTIFICATIONS", nameof(ShowNotif), true);
        this.ImageWidth = this.config.GetValue<int>("NOTIFICATIONS", nameof(ImageWidth), 136);
        this.ImageHeight = this.config.GetValue<int>("NOTIFICATIONS", nameof(ImageHeight), 27);
        this.StealthIfPlayerAiming = this.config.GetValue<bool>("STEALTHMODE", nameof(StealthIfPlayerAiming), false);
        this.StealthAutoDisable = this.config.GetValue<bool>("STEALTHMODE", nameof(StealthAutoDisable), true);
        if (this.ShowNotif)
            UI.Notify("SelectiveFire loaded.");
        this.WasteAmmo = this.config.GetValue<bool>("REALLISTICMAGS", nameof(WasteAmmo), false);
        this.BreathAimMovement = this.config.GetValue<bool>("REALLISTICAIMING", nameof(BreathAimMovement), false);
        this.BreathMovementRate = this.config.GetValue<int>("REALLISTICAIMING", nameof(BreathMovementRate), 1);
        this.ShakeWhenAim = this.config.GetValue<bool>("REALLISTICAIMING", nameof(ShakeWhenAim), false);
        this.ShakeWhenAimMovementRate = this.config.GetValue<int>("REALLISTICAIMING", nameof(ShakeWhenAimMovementRate), 1);
        ref Keys local = ref this.ChangeFireModeHotkey_Keys;
        Enum.TryParse<Keys>(str, out local);
    }

    private void OnTick(object sender, EventArgs e)
    {
        this.player = Game.Player.Character;
        Weapon current = this.player.Weapons.Current;
        if (this.ActivateSelectiveFire && (current.Hash == WeaponHash.AdvancedRifle || current.Hash == WeaponHash.APPistol || current.Hash == WeaponHash.AssaultRifle || current.Hash == WeaponHash.AssaultSMG || current.Hash == WeaponHash.BullpupRifle || current.Hash == WeaponHash.CarbineRifle || current.Hash == WeaponHash.CombatPDW || current.Hash == WeaponHash.Gusenberg || current.Hash == WeaponHash.MachinePistol || current.Hash == WeaponHash.MicroSMG || current.Hash == WeaponHash.SMG || current.Hash == WeaponHash.SpecialCarbine))
        {
            this.ammo = Game.Player.Character.Weapons.Current.Ammo;
            this.capableWeapon = true;
            if (this.fireMode == 1)
                this.firemodeImg = this.firemodeImgRoot + "fullauto.png";
            else if (this.fireMode == 2)
            {
                this.firemodeImg = this.firemodeImgRoot + "semiauto.png";
                this.SemiAutoMode();
            }
            else if (this.fireMode == 3)
            {
                this.firemodeImg = this.firemodeImgRoot + "burst3.png";
                this.BurstMode(this.ShotsPerBurst);
            }
            else if (this.fireMode == 4)
            {
                
                this.firemodeImg = this.firemodeImgRoot + "safety.png";
                SafetyMode();
            }
            if (this.ShowImage && this.AutoHideImage)
            {
                if (current != this.previousWeapon)
                    this.firemodechanged = true;
                if (this.firemodechanged)
                {
                    this.imgTimer = DateTime.Now;
                    this.firemodechanged = false;
                    this.showImg = true;
                }
                if ((DateTime.Now - this.imgTimer).TotalSeconds > (double)this.ImageShownTime)
                    this.showImg = false;
                if (this.showImg)
                    UI.DrawTexture(this.firemodeImg, 1, 1, 100, new Point(1, 1), new Size(this.ImageWidth, this.ImageHeight));
            }
            if (this.ShowImage && !this.AutoHideImage)
                UI.DrawTexture(this.firemodeImg, 1, 1, 100, new Point(1, 1), new Size(this.ImageWidth, this.ImageHeight));
            if (this.player.IsReloading || current != this.previousWeapon)
            {
                Game.Player.DisableFiringThisFrame();
                this.ammocount = 0;
            }
        }
        else
            this.capableWeapon = false;
        if (this.StealthIfPlayerAiming)
        {
            this.stealth = Function.Call<bool>(Hash._0x7C2AC9CA66575FBF, (InputArgument)this.player);
            if (this.StealthIfPlayerAiming && !this.stealthLaunchIfPlayerAiming && Game.Player.IsAiming)
            {
                this.StealthMode(true);
                this.stealthLaunchIfPlayerAiming = true;
            }
            else if (this.stealthLaunchIfPlayerAiming && !Game.Player.IsAiming)
            {
                this.stealthLaunchIfPlayerAiming = false;
                if (this.StealthAutoDisable && this.stealth)
                    this.StealthMode(false);
            }
        }
        if (this.WasteAmmo && (current.Hash == WeaponHash.AdvancedRifle || current.Hash == WeaponHash.APPistol || current.Hash == WeaponHash.AssaultRifle || current.Hash == WeaponHash.AssaultSMG || current.Hash == WeaponHash.BullpupRifle || current.Hash == WeaponHash.CarbineRifle || current.Hash == WeaponHash.CombatMG || current.Hash == WeaponHash.CombatPDW || current.Hash == WeaponHash.CombatPistol || current.Hash == WeaponHash.CompactRifle || current.Hash == WeaponHash.GrenadeLauncher || current.Hash == WeaponHash.GrenadeLauncherSmoke || current.Hash == WeaponHash.Gusenberg || current.Hash == WeaponHash.HeavyPistol || current.Hash == WeaponHash.HeavyShotgun || current.Hash == WeaponHash.HeavySniper || current.Hash == WeaponHash.MachinePistol || current.Hash == WeaponHash.MarksmanRifle || current.Hash == WeaponHash.MG || current.Hash == WeaponHash.MicroSMG || current.Hash == WeaponHash.Pistol || current.Hash == WeaponHash.Pistol50 || current.Hash == WeaponHash.Revolver || current.Hash == WeaponHash.SMG || current.Hash == WeaponHash.SniperRifle || current.Hash == WeaponHash.SNSPistol || current.Hash == WeaponHash.SpecialCarbine || current.Hash == WeaponHash.VintagePistol || current.Hash == WeaponHash.AssaultShotgun))
        {
            if (this.player.IsReloading && !this.playerreloaded)
            {
                this.playerreloaded = true;
                if (this.player.Weapons.Current.AmmoInClip > 0)
                    this.player.Weapons.Current.AmmoInClip = 0;
            }
            if (!this.player.IsReloading && this.player.Weapons.Current.AmmoInClip == this.player.Weapons.Current.DefaultClipSize)
                this.playerreloaded = false;
        }
        if (this.ShakeWhenAim && !this.BreathAimMovement)
        {
            if (Game.Player.IsAiming && !this.aimshaking)
            {
                GameplayCamera.Shake(CameraShake.Jolt, 0.1f * (float)this.ShakeWhenAimMovementRate);
                this.aimshaking = true;
            }
            else if (!Game.Player.IsAiming)
                this.aimshaking = false;
        }
        if (this.BreathAimMovement && !this.ShakeWhenAim)
        {
            if (Game.Player.IsAiming && !this.breathshaking)
            {
                GameplayCamera.Shake(CameraShake.Hand, 0.1f * (float)this.BreathMovementRate);
                this.breathshaking = true;
            }
            else if (!Game.Player.IsAiming)
            {
                GameplayCamera.StopShaking();
                this.breathshaking = false;
            }
        }
        if (this.BreathAimMovement && this.ShakeWhenAim)
        {
            if (Game.Player.IsAiming)
            {
                if (!this.aimshaking)
                {
                    GameplayCamera.Shake(CameraShake.Jolt, 0.1f * (float)this.ShakeWhenAimMovementRate);
                    this.aimshaking = true;
                }
                else if (!GameplayCamera.IsShaking)
                {
                    GameplayCamera.Shake(CameraShake.Hand, 0.1f * (float)this.BreathMovementRate);
                    this.breathshaking = true;
                }
            }
            else if (this.aimshaking || this.breathshaking)
            {
                GameplayCamera.StopShaking();
                this.aimshaking = false;
                this.breathshaking = false;
            }
        }
        this.previousWeapon = current;
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (!this.capableWeapon || e.KeyCode != this.ChangeFireModeHotkey_Keys)
            return;
        if (this.fireMode == 1)
        {
            this.fireMode = 2;
            if (this.ShowNotif)
                UI.Notify("Fire mode: SEMI-AUTO");
            this.firemodechanged = true;
            SelectorSwitchSoundFX();
        }
        else if (this.fireMode == 2)
        {
            this.fireMode = 3;
            if (this.ShowNotif)
                UI.Notify("Fire mode: BURST - x" + (object)this.ShotsPerBurst);
            this.firemodechanged = true;
            SelectorSwitchSoundFX();
        }
        else if (this.fireMode == 3)
        {
            this.fireMode = 4;
            if (this.ShowNotif)
                UI.Notify("Fire mode: FULL-AUTO");
            this.firemodechanged = true;
            SelectorSwitchSoundFX();

        }
        else
        {
            if (this.fireMode != 4)
                return;
            this.fireMode = 1;
            if (this.ShowNotif)
                UI.Notify("Fire mode: SAFETY");
            this.firemodechanged = true;
            SelectorSwitchSoundFX();
        }
    }
    /*
    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (!this.capableWeapon || e.KeyCode != this.ChangeFireModeHotkey_Keys)
            return;
        if (this.fireMode == 1)
        {
            this.fireMode = 2;
            if (this.ShowNotif)
                UI.Notify("Fire mode: SEMI-AUTO");
            this.firemodechanged = true;
            SelectorSwitchSoundFX();
        }
        else if (this.fireMode == 2)
        {
            this.fireMode = 3;
            if (this.ShowNotif)
                UI.Notify("Fire mode: BURST - x" + (object)this.ShotsPerBurst);
            this.firemodechanged = true;
            SelectorSwitchSoundFX();
        }
        else
        {
            if (this.fireMode != 3)
                return;
            this.fireMode = 1;
            if (this.ShowNotif)
                UI.Notify("Fire mode: FULL-AUTO");
            this.firemodechanged = true;
            SelectorSwitchSoundFX();
        }
    }
    */
    private void BurstMode(int SpB)
    {
        if (this.player.IsShooting)
            ++this.ammocount;
        if (this.ammocount < SpB && this.ammocount > 0)
        {
            Game.SetControlNormal(0, GTA.Control.Attack, 1f);
            if (this.player.IsAimingFromCover)
                Game.SetControlNormal(0, GTA.Control.Aim, 1f);
        }
        if (this.ammocount != SpB)
            return;
        Game.Player.DisableFiringThisFrame();
        if (!Game.IsControlJustReleased(0, GTA.Control.Attack))
            return;
        this.ammocount = 0;
    }

    private void SemiAutoMode()
    {
        if (!Game.IsControlPressed(0, GTA.Control.Attack))
            return;
        Game.Player.DisableFiringThisFrame();
    }

    private void SafetyMode()
    {
        Game.DisableControlThisFrame(0, GTA.Control.Attack);
        Game.DisableControlThisFrame(0, GTA.Control.Attack2);
        Game.Player.DisableFiringThisFrame();
        if (!Game.IsControlJustReleased(0, GTA.Control.Attack))
            return;
        DryFireSoundFX();
        
    }

    private void StealthMode(bool sth)
    {
        Function.Call(Hash._0x88CBB5CEB96B7BD2, (InputArgument)this.player, (InputArgument)sth, (InputArgument)0);
    }

    private void SelectorSwitchSoundFX()
    {
        if (File.Exists("scripts\\SelectiveFire\\Switch.wav"))
        {
            this.WavereaderDown = new WaveFileReader("scripts\\SelectiveFire\\Switch.wav");
            this.wavechanDown = new WaveChannel32((WaveStream)this.WavereaderDown);
            this.volumeDown = (float)this.gvd;
            this.DSODown = new DirectSoundOut();
            this.DSODown.Init((IWaveProvider)this.wavechanDown);
            this.wavechanDown.Volume = this.volumeDown / 100f;
            this.DSODown.Play();
            this.DSODown.Dispose();
        }
        else
            GTA.UI.Notify("Switch.wav is not found!");
    }

    private void DryFireSoundFX()
    {
        if (File.Exists("scripts\\SelectiveFire\\dryfire.wav"))
        {
            this.WavereaderDown = new WaveFileReader("scripts\\SelectiveFire\\dryfire.wav");
            this.wavechanDown = new WaveChannel32((WaveStream)this.WavereaderDown);
            this.volumeDown = (float)this.gvd;
            this.DSODown = new DirectSoundOut();
            this.DSODown.Init((IWaveProvider)this.wavechanDown);
            this.wavechanDown.Volume = this.volumeDown / 100f;
            this.DSODown.Play();
            this.DSODown.Dispose();
        }
        else
            GTA.UI.Notify("dryfire.wav is not found!");
    }

}
