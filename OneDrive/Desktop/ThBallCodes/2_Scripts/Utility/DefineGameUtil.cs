using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DefineGameUtil
{
    public struct CharacterAbilities
    {
        string NAME;
        float HP;
        float SPEED;
        float POWER;

        public string GetName
        {
            get { return NAME; }
        }
        public float GetHp
        {
            get { return HP; }
        }
        public float GetSpeed
        {
            get { return SPEED; }
        }
        public float GetPower
        {
            get { return POWER; }
        }

        public void SetCharAbil(string name, float hp, float sp, float power)
        {
            NAME = name;
            HP = hp;
            SPEED = sp;
            POWER = power;
        }
    }

    public enum eSceneType
    {
        Start = 0,
        Login,
        Lobby,
        SnowMap,
    }
    public enum eState
    {
        IDLE = 0,
        READY,
        WALK,
        RUN,
        THROW,
        HIT,
        DEATH,
        BLOCK,
        WIN,
    }
    public enum eItem
    {
        StarBall = 0,
        PoisonBall,
        SnowBall,
        ItemSpeedUpObj,
        ItemPowerUpObj,
        ItemPotionObj,
        ItemEyeObj,
        ItemTelePortObj,

        Max
    }
    public enum eParticle
    {
        BlockDestroy=0,
        HitStarBall,
        HitPoisonBall,
        HitSnowBall,
        MaxGauge,
    }

    public enum eBGM
    {
        Start = 0,
        Lobby,
        Room,
        Ingame
    }

    public enum eSFX
    {
        BtnClick=0,
        Hit_Player,
        Hit_Block,
        Hit_Default
    }
}
