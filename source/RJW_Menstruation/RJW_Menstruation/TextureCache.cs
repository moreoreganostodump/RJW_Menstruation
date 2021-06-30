using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace RJW_Menstruation
{
    [StaticConstructorOnStartup]
    public static class TextureCache
    {

        public static Texture2D milkTexture
        {
            get
            {
                if (milktexturecache == null) milktexturecache = SolidColorMaterials.NewSolidColorTexture(0.992f, 1.0f, 0.960f, 1.0f);
                return milktexturecache;
            }
        }
        public static Texture2D slaaneshTexture
        {
            get
            {
                if (slaaneshtexturecache == null) slaaneshtexturecache = SolidColorMaterials.NewSolidColorTexture(0.686f, 0.062f, 0.698f, 1.0f);
                return slaaneshtexturecache;
            }
        }
        public static Texture2D khorneTexture
        {
            get
            {
                if (khornetexturecache == null) khornetexturecache = SolidColorMaterials.NewSolidColorTexture(0.415f, 0.0f, 0.003f, 1.0f);
                return khornetexturecache;
            }
        }
        public static Texture2D tzeentchTexture
        {
            get
            {
                if (tzeentchtexturecache == null) tzeentchtexturecache = SolidColorMaterials.NewSolidColorTexture(0.082f, 0.453f, 0.6f, 1.0f);
                return tzeentchtexturecache;
            }
        }
        public static Texture2D nurgleTexture
        {
            get
            {
                if (nurgletexturecache == null) nurgletexturecache = SolidColorMaterials.NewSolidColorTexture(0.6f, 0.83f, 0.35f, 1.0f);
                return nurgletexturecache;
            }
        }
        public static Texture2D humanTexture
        {
            get
            {
                if (humantexturecache == null) humantexturecache = SolidColorMaterials.NewSolidColorTexture(0.878f, 0.674f, 0.411f, 1.0f);
                return humantexturecache;
            }
        }
        public static Texture2D animalTexture
        {
            get
            {
                if (animaltexturecache == null) animaltexturecache = SolidColorMaterials.NewSolidColorTexture(0.411f, 0.521f, 0.878f, 1.0f);
                return animaltexturecache;
            }
        }
        public static Texture2D fertilityTexture
        {
            get
            {
                if (fertilitytexturecache == null) fertilitytexturecache = SolidColorMaterials.NewSolidColorTexture(0.843f, 0.474f, 0.6f, 1.0f);
                return fertilitytexturecache;
            }
        }
        public static Texture2D ghalmarazTexture
        {
            get
            {
                if (ghalmaraztexturecache == null) ghalmaraztexturecache = SolidColorMaterials.NewSolidColorTexture(0.7f, 0.7f, 0.0f, 1.0f);
                return ghalmaraztexturecache;
            }
        }

        


        private static Texture2D milktexturecache = null;
        private static Texture2D slaaneshtexturecache = null;
        private static Texture2D khornetexturecache = null;
        private static Texture2D tzeentchtexturecache = null;
        private static Texture2D nurgletexturecache = null;
        private static Texture2D humantexturecache = null;
        private static Texture2D animaltexturecache = null;
        private static Texture2D fertilitytexturecache = null;
        private static Texture2D ghalmaraztexturecache = null;




    }
}
