using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FDK;

namespace DTXMania
{
    class EffekseerEffect : IDisposable
    {
        public EffekseerEffect( Effekseer effekseer, VariablePath efkPath, float magnification = 10.0f, VariablePath materialPath = null )
        {
            this._Effekseer = effekseer;
            this._Effect = EffekseerRendererDX11NET.Effect.Create( this._Effekseer.Manager, efkPath.変数なしパス, magnification, materialPath?.変数なしパス );
        }

        public void Play( float x, float y, float z )
        {
            this._EffectHandle = this._Effekseer.Manager.Play( this._Effect, x, y, z );
        }

        public void Stop()
        {
            this._Effekseer.Manager.StopEffect( this._EffectHandle );
        }

        public void Dispose()
        {
            this._Effect.Dispose();
            this._Effekseer = null;
        }


        private Effekseer _Effekseer;

        private EffekseerRendererDX11NET.Effect _Effect;

        private int _EffectHandle;
    }
}
