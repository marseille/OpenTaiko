﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.IO;
using FDK;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Input;

namespace TJAPlayer3
{
	internal class CStage起動 : CStage
	{
		// コンストラクタ

		public CStage起動()
		{
			base.eステージID = CStage.Eステージ.起動;
			base.b活性化してない = true;
		}

		public List<string> list進行文字列;

		// CStage 実装

		public override void On活性化()
		{
			Trace.TraceInformation( "起動ステージを活性化します。" );
			Trace.Indent();
			try
			{
				this.list進行文字列 = new List<string>();
				base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
				base.On活性化();
				Trace.TraceInformation( "起動ステージの活性化を完了しました。" );
			}
			finally
			{
				Trace.Unindent();
			}
		}
		public override void On非活性化()
		{
			Trace.TraceInformation( "起動ステージを非活性化します。" );
			Trace.Indent();
			try
			{
				this.list進行文字列 = null;
				if ( es != null )
				{
					if ( ( es.thDTXFileEnumerate != null ) && es.thDTXFileEnumerate.IsAlive )
					{
						Trace.TraceWarning( "リスト構築スレッドを強制停止します。" );
						es.thDTXFileEnumerate.Abort();
						es.thDTXFileEnumerate.Join();
					}
				}
				base.On非活性化();
				Trace.TraceInformation( "起動ステージの非活性化を完了しました。" );
			}
			finally
			{
				Trace.Unindent();
			}
		}
		public override void OnManagedリソースの作成()
		{
			if( !base.b活性化してない )
			{
				this.tx背景 = TJAPlayer3.tテクスチャの生成( CSkin.Path( @"Graphics\1_Title\Background.png" ), false );
				base.OnManagedリソースの作成();
			}
		}
		public override void OnManagedリソースの解放()
		{
			if( !base.b活性化してない )
			{
				TJAPlayer3.tテクスチャの解放( ref this.tx背景 );
				base.OnManagedリソースの解放();
			}
		}
		public override int On進行描画()
		{
			if( !base.b活性化してない )
			{
				if( base.b初めての進行描画 )
				{
					this.list進行文字列.Add( "DTXManiaXG Ver.K powered by YAMAHA Silent Session Drums\n" );
					this.list進行文字列.Add( "Product by.kairera0467\n" );
					this.list進行文字列.Add( "Release: " + TJAPlayer3.VERSION + " [" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + "]" );

                    this.list進行文字列.Add("");
                    this.list進行文字列.Add("TJAPlayer3-Develop-ReWrite forked TJAPlayer3(@aioilight)");
					this.list進行文字列.Add("OpenTaiko forked TJAPlayer3-Develop-ReWrite(@TouhouRenren)");
					this.list進行文字列.Add("OpenTaiko edited by 0AuBSQ");
                    this.list進行文字列.Add("");

                    es = new CEnumSongs();
					es.StartEnumFromCache();
					base.b初めての進行描画 = false;
					return 0;
				}


				#region [ this.str現在進行中 の決定 ]
				//-----------------
				switch( base.eフェーズID )
				{
					case CStage.Eフェーズ.起動0_システムサウンドを構築:
						this.str現在進行中 = "SYSTEM SOUND...";
						break;

					case CStage.Eフェーズ.起動00_songlistから曲リストを作成する:
						this.str現在進行中 = "SONG LIST...";
						break;

					case CStage.Eフェーズ.起動2_曲を検索してリストを作成する:
						this.str現在進行中 = string.Format( "{0} ... {1}", "Enumerating songs", es.Songs管理.n検索されたスコア数 );
						break;

					case CStage.Eフェーズ.起動3_スコアキャッシュをリストに反映する:
						this.str現在進行中 = string.Format( "{0} ... {1}/{2}", "Loading score properties from songs.db", es.Songs管理.nスコアキャッシュから反映できたスコア数, es.Songs管理.n検索されたスコア数 );
						break;

					case CStage.Eフェーズ.起動4_スコアキャッシュになかった曲をファイルから読み込んで反映する:
						this.str現在進行中 = string.Format( "{0} ... {1}/{2}", "Loading score properties from files", es.Songs管理.nファイルから反映できたスコア数, es.Songs管理.n検索されたスコア数 - es.Songs管理.nスコアキャッシュから反映できたスコア数 );
						break;

					case CStage.Eフェーズ.起動5_曲リストへ後処理を適用する:
						this.str現在進行中 = string.Format( "{0} ... ", "Building songlists" );
						break;

					case CStage.Eフェーズ.起動_テクスチャの読み込み:
                        this.list進行文字列.Add("LOADING TEXTURES...");
                        TJAPlayer3.Tx.LoadTexture();
                        this.list進行文字列.Add("LOADING TEXTURES...OK");
                        this.str現在進行中 = "Setup done.";
						this.eフェーズID = Eフェーズ.起動7_完了;
                        break;
				}
				//-----------------
				#endregion

				if(eフェーズID != Eフェーズ.起動7_完了)
                {
					#region [ this.list進行文字列＋this.現在進行中 の表示 ]
					//-----------------
					lock (this.list進行文字列)
					{
						int x = 320;
						int y = 20;
						foreach (string str in this.list進行文字列)
						{
							TJAPlayer3.act文字コンソール.tPrint(x, y, C文字コンソール.Eフォント種別.白, str);
							y += 24;
						}
						TJAPlayer3.act文字コンソール.tPrint(x, y, C文字コンソール.Eフォント種別.白, this.str現在進行中);
					}
					//-----------------
					#endregion
				}
                else
                {
					TJAPlayer3.Tx.Readme.t2D描画(TJAPlayer3.app.Device, 0, 0);
				}

				if ( es != null && es.IsSongListEnumCompletelyDone)							// 曲リスト作成が終わったら
				{
					TJAPlayer3.Songs管理 = ( es != null ) ? es.Songs管理 : null;		// 最後に、曲リストを拾い上げる

					if(TJAPlayer3.Input管理.Keyboard.bキーが押された((int)SlimDXKeys.Key.Return))
                    {
						TJAPlayer3.Skin.sound決定音.t再生する();
						return 1;
                    }
				}
			}
			return 0;
		}


		// その他

		#region [ private ]
		//-----------------
		private string str現在進行中 = "";
		private CTexture tx背景;
		private CEnumSongs es;

		#endregion
	}
}
