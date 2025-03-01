﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using TJAPlayer3.C曲リストノードComparers;
using FDK;
using System.Web.UI;
using System.Drawing;

namespace TJAPlayer3
{
	[Serializable]
	internal class CSongs管理
	{

		public int nスコアキャッシュから反映できたスコア数 
		{
			get;
			set; 
		}
		public int nファイルから反映できたスコア数
		{
			get;
			set;
		}
		public int n検索されたスコア数 
		{ 
			get;
			set;
		}
		public int n検索された曲ノード数
		{
			get; 
			set;
		}

		public List<C曲リストノード> list曲ルート;			// 起動時にフォルダ検索して構築されるlist
		public List<C曲リストノード> list曲ルート_Dan = new List<C曲リストノード>();			// 起動時にフォルダ検索して構築されるlist
		public bool bIsSuspending							// 外部スレッドから、内部スレッドのsuspendを指示する時にtrueにする
		{													// 再開時は、これをfalseにしてから、次のautoReset.Set()を実行する
			get;
			set;
		}
		public bool bIsSlowdown								// #PREMOVIE再生時に曲検索を遅くする
		{
			get;
			set;
		}
		[NonSerialized]
		public AutoResetEvent AutoReset;		

		private int searchCount;							// #PREMOVIE中は検索n回実行したら少しスリープする

		// コンストラクタ

		public CSongs管理()
		{
			this.list曲ルート = new List<C曲リストノード>();
			this.n検索された曲ノード数 = 0;
			this.n検索されたスコア数 = 0;
			this.bIsSuspending = false;						// #27060
			this.AutoReset = new AutoResetEvent( true );	// #27060
			this.searchCount = 0;
		}


		// メソッド

		#region [ Fetch song list ]
		//-----------------

		public void UpdateDownloadBox()
		{

			C曲リストノード downloadBox = null;
			for (int i = 0; i < TJAPlayer3.Songs管理.list曲ルート.Count; i++)
			{
				if (TJAPlayer3.Songs管理.list曲ルート[i].strジャンル == "Download")
				{
					downloadBox = TJAPlayer3.Songs管理.list曲ルート[i];
					if (downloadBox.r親ノード != null) downloadBox = downloadBox.r親ノード;
				}

			}

			if (downloadBox != null && downloadBox.list子リスト != null)
            {

				var flatten = TJAPlayer3.stage選曲.act曲リスト.flattenList(downloadBox.list子リスト);
				
				// Works because flattenList creates a new List
				for (int i = 0; i < downloadBox.list子リスト.Count; i++)
				{
					CSongDict.tRemoveSongNode(downloadBox.list子リスト[i].uniqueId);
					downloadBox.list子リスト.Remove(downloadBox.list子リスト[i]);
					i--;
				}
				

				var path = downloadBox.arスコア[0].ファイル情報.フォルダの絶対パス;

				if (flatten.Count > 0)
				{
					int index = list曲ルート.IndexOf(flatten[0]);

					if (!list曲ルート.Contains(downloadBox))
					{
						this.list曲ルート = this.list曲ルート.Except(flatten).ToList();
						list曲ルート.Insert(index, downloadBox);
					}

					t曲を検索してリストを作成する(path, true, downloadBox.list子リスト, downloadBox);
					this.t曲リストへ後処理を適用する(downloadBox.list子リスト, $"/{downloadBox.strタイトル}/");
					tSongsDBになかった曲をファイルから読み込んで反映する(downloadBox.list子リスト);
					downloadBox.list子リスト.Insert(0, CSongDict.tGenerateBackButton(downloadBox, $"/{downloadBox.strタイトル}/"));
				}
			}
			
		}
		public void t曲を検索してリストを作成する( string str基点フォルダ, bool b子BOXへ再帰する )
		{
			this.t曲を検索してリストを作成する( str基点フォルダ, b子BOXへ再帰する, this.list曲ルート, null );
		}
		private void t曲を検索してリストを作成する( string str基点フォルダ, bool b子BOXへ再帰する, List<C曲リストノード> listノードリスト, C曲リストノード node親 )
		{
			//This does actually get called
			if( !str基点フォルダ.EndsWith( @"\" ) )
				str基点フォルダ = str基点フォルダ + @"\";

			DirectoryInfo info = new DirectoryInfo( str基点フォルダ );

			if( TJAPlayer3.ConfigIni.bLog曲検索ログ出力 )
				Trace.TraceInformation( "基点フォルダ: " + str基点フォルダ );

			foreach( FileInfo fileinfo in info.GetFiles() )
			{
				SlowOrSuspendSearchTask();		// #27060 中断要求があったら、解除要求が来るまで待機, #PREMOVIE再生中は検索負荷を落とす
				string strExt = fileinfo.Extension.ToLower();

                if( ( strExt.Equals( ".tja" ) || strExt.Equals( ".dtx" ) ) )
                {                

                    #region[ 新処理 ]
                    CDTX dtx = new CDTX( str基点フォルダ + fileinfo.Name, false, 1.0, 0, 0 );
                    C曲リストノード c曲リストノード = new C曲リストノード();
                    c曲リストノード.eノード種別 = C曲リストノード.Eノード種別.SCORE;

                    bool b = false;
                    for( int n = 0; n < (int)Difficulty.Total; n++ )
                    {
                        if( dtx.b譜面が存在する[ n ] )
                        {
                            c曲リストノード.nスコア数++;
                            c曲リストノード.r親ノード = node親;
                            c曲リストノード.strBreadcrumbs = ( c曲リストノード.r親ノード == null ) ?
                                str基点フォルダ + fileinfo.Name : c曲リストノード.r親ノード.strBreadcrumbs + " > " + str基点フォルダ + fileinfo.Name;

                            c曲リストノード.strタイトル = dtx.TITLE;
                            c曲リストノード.strサブタイトル = dtx.SUBTITLE;

							if (dtx.List_DanSongs != null)
								c曲リストノード.DanSongs = dtx.List_DanSongs;

							if (dtx.Dan_C != null)
								c曲リストノード.Dan_C = dtx.Dan_C;

							//genre MUST BE SET otherwise it might be null and cause NO SONGS FOUND
							//silent mega error.

							//Precedence:
							//song folder .tja genre
							//genre folder box.def
							//default														
							
							if (!string.IsNullOrEmpty(dtx.GENRE))
							{
								c曲リストノード.strジャンル = dtx.GENRE;
								c曲リストノード.str本当のジャンル = dtx.GENRE;
							}
							else
							{

								if (c曲リストノード.r親ノード != null)
								{
									if (!string.IsNullOrEmpty(c曲リストノード.r親ノード.strジャンル))
									{
										c曲リストノード.strジャンル = c曲リストノード.r親ノード.strジャンル;
										c曲リストノード.str本当のジャンル = c曲リストノード.r親ノード.strジャンル;
									}
								}
								else
								{
									c曲リストノード.strジャンル = "MISSING GENRE";
									c曲リストノード.str本当のジャンル = "MISSING GENRE";
								}
							}

							if (c曲リストノード.r親ノード != null)
                            {
                                if (c曲リストノード.r親ノード.IsChangedForeColor)
                                {
                                    c曲リストノード.ForeColor = c曲リストノード.r親ノード.ForeColor;
                                    c曲リストノード.IsChangedForeColor = true;
                                }
                                if (c曲リストノード.r親ノード.IsChangedBackColor)
                                {
                                    c曲リストノード.BackColor = c曲リストノード.r親ノード.BackColor;
                                    c曲リストノード.IsChangedBackColor = true;
                                }
								if (c曲リストノード.r親ノード.isChangedBoxColor)
                                {
									c曲リストノード.BoxColor = c曲リストノード.r親ノード.BoxColor;
									c曲リストノード.isChangedBoxColor = true;
                                }
								if (c曲リストノード.r親ノード.isChangedBgColor)
								{
									c曲リストノード.BgColor = c曲リストノード.r親ノード.BgColor;
									c曲リストノード.isChangedBgColor = true;
								}
								if (c曲リストノード.r親ノード.isChangedBgType)
                                {
									c曲リストノード.BgType = c曲リストノード.r親ノード.BgType;
									c曲リストノード.isChangedBgType = true;
								}
								if (c曲リストノード.r親ノード.isChangedBoxType)
                                {
									c曲リストノード.BoxType = c曲リストノード.r親ノード.BoxType;
									c曲リストノード.isChangedBoxType = true;
								}
								if (c曲リストノード.r親ノード.isChangedBoxChara)
								{
									c曲リストノード.BoxChara = c曲リストノード.r親ノード.BoxChara;
									c曲リストノード.isChangedBoxChara = true;
								}
							}


                            switch (CStrジャンルtoNum.ForAC15(c曲リストノード.strジャンル))
                            {
                                case 0:
                                    c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_JPOP;
                                    c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_JPOP;
                                    break;
                                case 1:
                                    c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Anime;
                                    c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Anime;
                                    break;
                                case 2:
                                    c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_VOCALOID;
                                    c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_VOCALOID;
                                    break;
                                case 3:
                                    c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Children;
                                    c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Children;
                                    break;
                                case 4:
                                    c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Variety;
                                    c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Variety;
                                    break;
                                case 5:
                                    c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Classic;
                                    c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Classic;
                                    break;
                                case 6:
                                    c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_GameMusic;
                                    c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_GameMusic;
                                    break;
                                case 7:
                                    c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Namco;
                                    c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Namco;
                                    break;
                                default:
                                    break;
                            }


                            c曲リストノード.nLevel = dtx.LEVELtaiko;
							c曲リストノード.uniqueId = dtx.uniqueID;

							CSongDict.tAddSongNode(c曲リストノード.uniqueId, c曲リストノード);

							c曲リストノード.arスコア[ n ] = new Cスコア();
                            c曲リストノード.arスコア[ n ].ファイル情報.ファイルの絶対パス = str基点フォルダ + fileinfo.Name;
                            c曲リストノード.arスコア[ n ].ファイル情報.フォルダの絶対パス = str基点フォルダ;
                            c曲リストノード.arスコア[ n ].ファイル情報.ファイルサイズ = fileinfo.Length;
                            c曲リストノード.arスコア[ n ].ファイル情報.最終更新日時 = fileinfo.LastWriteTime;
                            string strFileNameScoreIni = c曲リストノード.arスコア[ n ].ファイル情報.ファイルの絶対パス + ".score.ini";
                            if( File.Exists( strFileNameScoreIni ) )
                            {
                                FileInfo infoScoreIni = new FileInfo( strFileNameScoreIni );
                                c曲リストノード.arスコア[ n ].ScoreIni情報.ファイルサイズ = infoScoreIni.Length;
                                c曲リストノード.arスコア[ n ].ScoreIni情報.最終更新日時 = infoScoreIni.LastWriteTime;
                            }
                            if( b == false )
                            {
                                this.n検索されたスコア数++;
                                listノードリスト.Add( c曲リストノード );
                                this.n検索された曲ノード数++;
                                b = true;
                            }
                        }
                    }
                    #endregion
                }
			}

			foreach( DirectoryInfo infoDir in info.GetDirectories() )
			{
				SlowOrSuspendSearchTask();		// #27060 中断要求があったら、解除要求が来るまで待機, #PREMOVIE再生中は検索負荷を落とす				

				#region [ b.box.def を含むフォルダの場合  ]
				//-----------------------------
				if( File.Exists( infoDir.FullName + @"\box.def" ) )
				{
					CBoxDef boxdef = new CBoxDef( infoDir.FullName + @"\box.def" );
					C曲リストノード c曲リストノード = new C曲リストノード();
					c曲リストノード.eノード種別 = C曲リストノード.Eノード種別.BOX;
					c曲リストノード.bDTXFilesで始まるフォルダ名のBOXである = false;
					c曲リストノード.strタイトル = boxdef.Title;
					c曲リストノード.strジャンル = boxdef.Genre;

					if (boxdef.IsChangedForeColor)
                    {
                        c曲リストノード.ForeColor = boxdef.ForeColor;
                        c曲リストノード.IsChangedForeColor = true;
                    }
                    if (boxdef.IsChangedBackColor)
                    {
                        c曲リストノード.BackColor = boxdef.BackColor;
                        c曲リストノード.IsChangedBackColor = true;
                    }
					if (boxdef.IsChangedBoxColor)
                    {
						c曲リストノード.BoxColor = boxdef.BoxColor;
						c曲リストノード.isChangedBoxColor = true;
                    }
					if (boxdef.IsChangedBgColor)
					{
						c曲リストノード.BgColor = boxdef.BgColor;
						c曲リストノード.isChangedBgColor = true;
					}
					if (boxdef.IsChangedBgType)
					{
						c曲リストノード.BgType = boxdef.BgType;
						c曲リストノード.isChangedBgType = true;
					}
					if (boxdef.IsChangedBoxType)
					{
						c曲リストノード.BoxType = boxdef.BoxType;
						c曲リストノード.isChangedBoxType = true;
					}
					if (boxdef.IsChangedBoxChara)
					{
						c曲リストノード.BoxChara = boxdef.BoxChara;
						c曲リストノード.isChangedBoxChara = true;
					}


					for (int i = 0; i < 3; i++)
					{
						if ((boxdef.BoxExplanation[i] != null) && (boxdef.BoxExplanation[i].Length > 0))
						{
							c曲リストノード.strBoxText[i] = boxdef.BoxExplanation[i];
						}
					}
					switch (CStrジャンルtoNum.ForAC15(c曲リストノード.strジャンル))
                    {
                        case 0:
                            c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_JPOP;
                            c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_JPOP;
                            break;
                        case 1:
                            c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Anime;
                            c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Anime;
                            break;
                        case 2:
                            c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_VOCALOID;
                            c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_VOCALOID;
                            break;
                        case 3:
                            c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Children;
                            c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Children;
                            break;
                        case 4:
                            c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Variety;
                            c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Variety;
                            break;
                        case 5:
                            c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Classic;
                            c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Classic;
                            break;
                        case 6:
                            c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_GameMusic;
                            c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_GameMusic;
                            break;
                        case 7:
                            c曲リストノード.ForeColor = TJAPlayer3.Skin.SongSelect_ForeColor_Namco;
                            c曲リストノード.BackColor = TJAPlayer3.Skin.SongSelect_BackColor_Namco;
                            break;
                        default:
                            break;
                    }

                    c曲リストノード.nスコア数 = 1;
					c曲リストノード.arスコア[ 0 ] = new Cスコア();
					c曲リストノード.arスコア[ 0 ].ファイル情報.フォルダの絶対パス = infoDir.FullName + @"\";
					c曲リストノード.arスコア[ 0 ].譜面情報.タイトル = boxdef.Title;
					c曲リストノード.arスコア[ 0 ].譜面情報.ジャンル = boxdef.Genre;
					c曲リストノード.r親ノード = node親;

					c曲リストノード.strBreadcrumbs = ( c曲リストノード.r親ノード == null ) ?
						c曲リストノード.strタイトル : c曲リストノード.r親ノード.strBreadcrumbs + " > " + c曲リストノード.strタイトル;
	
					
					c曲リストノード.list子リスト = new List<C曲リストノード>();
					listノードリスト.Add( c曲リストノード );
					if( TJAPlayer3.ConfigIni.bLog曲検索ログ出力 )
					{
						Trace.TraceInformation( "box.def検出 : {0}", infoDir.FullName + @"\box.def" );
						Trace.Indent();
						try
						{
							StringBuilder sb = new StringBuilder( 0x400 );
							sb.Append( string.Format( "nID#{0:D3}", c曲リストノード.nID ) );
							if( c曲リストノード.r親ノード != null )
							{
								sb.Append( string.Format( "(in#{0:D3}):", c曲リストノード.r親ノード.nID ) );
							}
							else
							{
								sb.Append( "(onRoot):" );
							}
							sb.Append( "BOX, Title=" + c曲リストノード.strタイトル );
							if( ( c曲リストノード.strジャンル != null ) && ( c曲リストノード.strジャンル.Length > 0 ) )
							{
								sb.Append( ", Genre=" + c曲リストノード.strジャンル );
							}
                            if (c曲リストノード.IsChangedForeColor)
                            {
                                sb.Append(", ForeColor=" + c曲リストノード.ForeColor.ToString());
                            }
                            if (c曲リストノード.IsChangedBackColor)
                            {
                                sb.Append(", BackColor=" + c曲リストノード.BackColor.ToString());
                            }
							if (c曲リストノード.isChangedBoxColor)
                            {
								sb.Append(", BoxColor=" + c曲リストノード.BoxColor.ToString());
                            }
							if (c曲リストノード.isChangedBgColor)
							{
								sb.Append(", BgColor=" + c曲リストノード.BgColor.ToString());
							}
							if (c曲リストノード.isChangedBoxType)
							{
								sb.Append(", BoxType=" + c曲リストノード.BoxType.ToString());
							}
							if (c曲リストノード.isChangedBgType)
							{
								sb.Append(", BgType=" + c曲リストノード.BgType.ToString());
							}
							if (c曲リストノード.isChangedBoxChara)
							{
								sb.Append(", BoxChara=" + c曲リストノード.BoxChara.ToString());
							}
							Trace.TraceInformation( sb.ToString() );
						}
						finally
						{
							Trace.Unindent();
						}
					}
					if( b子BOXへ再帰する )
					{
						this.t曲を検索してリストを作成する( infoDir.FullName + @"\", b子BOXへ再帰する, c曲リストノード.list子リスト, c曲リストノード );
					}
				}
				//-----------------------------
				#endregion

				#region [ c.通常フォルダの場合 ]
				//-----------------------------
				else
				{
					this.t曲を検索してリストを作成する( infoDir.FullName + @"\", b子BOXへ再帰する, listノードリスト, node親 );
				}
				//-----------------------------
				#endregion
			}
		}
		//-----------------
		#endregion
		
		#region [ SongsDBになかった曲をファイルから読み込んで反映する ]
		//-----------------
		public void tSongsDBになかった曲をファイルから読み込んで反映する()
		{
			this.nファイルから反映できたスコア数 = 0;
			this.tSongsDBになかった曲をファイルから読み込んで反映する( this.list曲ルート );
		}
		private void tSongsDBになかった曲をファイルから読み込んで反映する( List<C曲リストノード> ノードリスト )
		{
			foreach( C曲リストノード c曲リストノード in ノードリスト )
			{
				SlowOrSuspendSearchTask();		// #27060 中断要求があったら、解除要求が来るまで待機, #PREMOVIE再生中は検索負荷を落とす

				if( c曲リストノード.eノード種別 == C曲リストノード.Eノード種別.BOX )
				{
					this.tSongsDBになかった曲をファイルから読み込んで反映する( c曲リストノード.list子リスト );
				}
				else if( ( c曲リストノード.eノード種別 == C曲リストノード.Eノード種別.SCORE )
					  || ( c曲リストノード.eノード種別 == C曲リストノード.Eノード種別.SCORE_MIDI ) )
				{
					for( int i = 0; i < (int)Difficulty.Total; i++ )
					{
						if( ( c曲リストノード.arスコア[ i ] != null ) && !c曲リストノード.arスコア[ i ].bSongDBにキャッシュがあった )
						{
							#region [ DTX ファイルのヘッダだけ読み込み、Cスコア.譜面情報 を設定する ]
							//-----------------
							string path = c曲リストノード.arスコア[ i ].ファイル情報.ファイルの絶対パス;
							if( File.Exists( path ) )
							{
								try
								{
									CDTX cdtx = new CDTX( c曲リストノード.arスコア[ i ].ファイル情報.ファイルの絶対パス, true, 0, 0, 0 );
                                    if( File.Exists( c曲リストノード.arスコア[ i ].ファイル情報.フォルダの絶対パス + "set.def" ) )
									    cdtx = new CDTX( c曲リストノード.arスコア[ i ].ファイル情報.ファイルの絶対パス, true, 0, 0, 1 );

									c曲リストノード.arスコア[ i ].譜面情報.タイトル = cdtx.TITLE;
                                    
									
                                    c曲リストノード.arスコア[ i ].譜面情報.アーティスト名 = cdtx.ARTIST;
									c曲リストノード.arスコア[ i ].譜面情報.コメント = cdtx.COMMENT;
									c曲リストノード.arスコア[ i ].譜面情報.ジャンル = cdtx.GENRE;
                                    c曲リストノード.arスコア[ i ].譜面情報.Preimage = cdtx.PREIMAGE;
									c曲リストノード.arスコア[ i ].譜面情報.Presound = cdtx.PREVIEW;
									c曲リストノード.arスコア[ i ].譜面情報.Backgound = ( ( cdtx.BACKGROUND != null ) && ( cdtx.BACKGROUND.Length > 0 ) ) ? cdtx.BACKGROUND : cdtx.BACKGROUND_GR;
									c曲リストノード.arスコア[ i ].譜面情報.レベル.Drums = cdtx.LEVEL.Drums;
									c曲リストノード.arスコア[ i ].譜面情報.レベル.Guitar = cdtx.LEVEL.Guitar;
									c曲リストノード.arスコア[ i ].譜面情報.レベル.Bass = cdtx.LEVEL.Bass;
									c曲リストノード.arスコア[ i ].譜面情報.レベルを非表示にする = cdtx.HIDDENLEVEL;
									c曲リストノード.arスコア[ i ].譜面情報.Bpm = cdtx.BPM;
									c曲リストノード.arスコア[ i ].譜面情報.Duration = 0;	//  (cdtx.listChip == null)? 0 : cdtx.listChip[ cdtx.listChip.Count - 1 ].n発声時刻ms;
                                    c曲リストノード.arスコア[ i ].譜面情報.strBGMファイル名 = cdtx.strBGM_PATH;
                                    c曲リストノード.arスコア[ i ].譜面情報.SongVol = cdtx.SongVol;
                                    c曲リストノード.arスコア[ i ].譜面情報.SongLoudnessMetadata = cdtx.SongLoudnessMetadata;
								    c曲リストノード.arスコア[ i ].譜面情報.nデモBGMオフセット = cdtx.nデモBGMオフセット;
                                    c曲リストノード.arスコア[ i ].譜面情報.b譜面分岐[0] = cdtx.bHIDDENBRANCH ? false : cdtx.bHasBranch[ 0 ];
                                    c曲リストノード.arスコア[ i ].譜面情報.b譜面分岐[1] = cdtx.bHIDDENBRANCH ? false : cdtx.bHasBranch[ 1 ];
                                    c曲リストノード.arスコア[ i ].譜面情報.b譜面分岐[2] = cdtx.bHIDDENBRANCH ? false : cdtx.bHasBranch[ 2 ];
                                    c曲リストノード.arスコア[ i ].譜面情報.b譜面分岐[3] = cdtx.bHIDDENBRANCH ? false : cdtx.bHasBranch[ 3 ];
                                    c曲リストノード.arスコア[i].譜面情報.b譜面分岐[4] = cdtx.bHIDDENBRANCH ? false : cdtx.bHasBranch[4];
                                    c曲リストノード.arスコア[i].譜面情報.b譜面分岐[5] = cdtx.bHIDDENBRANCH ? false : cdtx.bHasBranch[5];
                                    c曲リストノード.arスコア[i].譜面情報.b譜面分岐[6] = cdtx.bHIDDENBRANCH ? false : cdtx.bHasBranch[6];
                                    c曲リストノード.arスコア[ i ].譜面情報.strサブタイトル = cdtx.SUBTITLE;
                                    c曲リストノード.arスコア[ i ].譜面情報.nレベル[0] = cdtx.LEVELtaiko[0];
                                    c曲リストノード.arスコア[ i ].譜面情報.nレベル[1] = cdtx.LEVELtaiko[1];
                                    c曲リストノード.arスコア[ i ].譜面情報.nレベル[2] = cdtx.LEVELtaiko[2];
                                    c曲リストノード.arスコア[ i ].譜面情報.nレベル[3] = cdtx.LEVELtaiko[3];
                                    c曲リストノード.arスコア[ i ].譜面情報.nレベル[4] = cdtx.LEVELtaiko[4];
                                    c曲リストノード.arスコア[i].譜面情報.nレベル[5] = cdtx.LEVELtaiko[5];
                                    c曲リストノード.arスコア[i].譜面情報.nレベル[6] = cdtx.LEVELtaiko[6];

									// Tower Lives
									c曲リストノード.arスコア[i].譜面情報.nLife = cdtx.LIFE;

									c曲リストノード.arスコア[i].譜面情報.nTowerType = cdtx.TOWERTYPE;

									c曲リストノード.arスコア[i].譜面情報.nDanTick = cdtx.DANTICK;
									c曲リストノード.arスコア[i].譜面情報.cDanTickColor = cdtx.DANTICKCOLOR;

									c曲リストノード.arスコア[i].譜面情報.nTotalFloor = 0;
									for (int k = 0; k < cdtx.listChip.Count; k++)
									{
										CDTX.CChip pChip = cdtx.listChip[k];

										if (pChip.n整数値_内部番号 > c曲リストノード.arスコア[i].譜面情報.nTotalFloor && pChip.nチャンネル番号 == 0x50)
											c曲リストノード.arスコア[i].譜面情報.nTotalFloor = pChip.n整数値_内部番号;
									}
									c曲リストノード.arスコア[i].譜面情報.nTotalFloor++;



									this.nファイルから反映できたスコア数++;
									cdtx.On非活性化();
//Debug.WriteLine( "★" + this.nファイルから反映できたスコア数 + " " + c曲リストノード.arスコア[ i ].譜面情報.タイトル );
									#region [ 曲検索ログ出力 ]
									//-----------------
									if( TJAPlayer3.ConfigIni.bLog曲検索ログ出力 )
									{
										StringBuilder sb = new StringBuilder( 0x400 );
										sb.Append( string.Format( "曲データファイルから譜面情報を転記しました。({0})", path ) );
										sb.Append( "(title=" + c曲リストノード.arスコア[ i ].譜面情報.タイトル );
										sb.Append( ", artist=" + c曲リストノード.arスコア[ i ].譜面情報.アーティスト名 );
										sb.Append( ", comment=" + c曲リストノード.arスコア[ i ].譜面情報.コメント );
										sb.Append( ", genre=" + c曲リストノード.arスコア[ i ].譜面情報.ジャンル );
										sb.Append( ", preimage=" + c曲リストノード.arスコア[ i ].譜面情報.Preimage );
										sb.Append( ", premovie=" + c曲リストノード.arスコア[ i ].譜面情報.Premovie );
										sb.Append( ", presound=" + c曲リストノード.arスコア[ i ].譜面情報.Presound );
										sb.Append( ", background=" + c曲リストノード.arスコア[ i ].譜面情報.Backgound );
										sb.Append( ", lvDr=" + c曲リストノード.arスコア[ i ].譜面情報.レベル.Drums );
										sb.Append( ", lvGt=" + c曲リストノード.arスコア[ i ].譜面情報.レベル.Guitar );
										sb.Append( ", lvBs=" + c曲リストノード.arスコア[ i ].譜面情報.レベル.Bass );
										sb.Append( ", lvHide=" + c曲リストノード.arスコア[ i ].譜面情報.レベルを非表示にする );
										sb.Append( ", type=" + c曲リストノード.arスコア[ i ].譜面情報.曲種別 );
										sb.Append( ", bpm=" + c曲リストノード.arスコア[ i ].譜面情報.Bpm );
									//	sb.Append( ", duration=" + c曲リストノード.arスコア[ i ].譜面情報.Duration );
										Trace.TraceInformation( sb.ToString() );
									}
									//-----------------
									#endregion
								}
								catch( Exception exception )
								{
									Trace.TraceError( exception.ToString() );
									c曲リストノード.arスコア[ i ] = null;
									c曲リストノード.nスコア数--;
									this.n検索されたスコア数--;
									Trace.TraceError( "曲データファイルの読み込みに失敗しました。({0})", path );
								}
							}
							//-----------------
							#endregion

							#region [ 対応する .score.ini が存在していれば読み込み、Cスコア.譜面情報 に追加設定する ]
							//-----------------
                            try
                            {
								var scoreIniPath = c曲リストノード.arスコア[i].ファイル情報.ファイルの絶対パス;// + ".score.ini";

                                if( File.Exists( scoreIniPath ) )
                                {
									this.tScoreIniを読み込んで譜面情報を設定する(scoreIniPath, c曲リストノード.arスコア[i]);
								}
								// Legacy save files from DTX mania
								/*
                                else
                                {
                                    string[] dtxscoreini = Directory.GetFiles(c曲リストノード.arスコア[i].ファイル情報.フォルダの絶対パス, "*.dtx.score.ini");
                                    if (dtxscoreini.Length != 0 && File.Exists(dtxscoreini[0]))
                                    {
                                        this.tScoreIniを読み込んで譜面情報を設定する(dtxscoreini[0], c曲リストノード.arスコア[i]);
                                    }
                                }
								*/
                            }
                            catch (Exception e)
                            {
                                Trace.TraceError( e.ToString() );
                                Trace.TraceError( "例外が発生しましたが処理を継続します。 (c8b6538c-46a1-403e-8cc3-fc7e7ff914fb)" );
                            }

							//-----------------
							#endregion
						}
					}
				}
			}
		}
		//-----------------
		#endregion

		#region [ 曲リストへ後処理を適用する ]
		//-----------------
		public void t曲リストへ後処理を適用する()
		{
			listStrBoxDefSkinSubfolderFullName = new List<string>();
			if ( TJAPlayer3.Skin.strBoxDefSkinSubfolders != null )
			{
				foreach ( string b in TJAPlayer3.Skin.strBoxDefSkinSubfolders )
				{
					listStrBoxDefSkinSubfolderFullName.Add( b );
				}
			}

			this.t曲リストへ後処理を適用する(this.list曲ルート);

			foreach (C曲リストノード c曲リストノード in list曲ルート)
			{
				if (c曲リストノード.eノード種別 == C曲リストノード.Eノード種別.BOX)
				{

					if (c曲リストノード.strジャンル == "段位道場")
					{
						if (TJAPlayer3.ConfigIni.bDanTowerHide)
							list曲ルート.Remove(c曲リストノード);

						// Add to dojo
						for (int i = 0; i < c曲リストノード.list子リスト.Count; i++)
						{
							if(c曲リストノード.list子リスト[i].eノード種別 == C曲リストノード.Eノード種別.SCORE)
							{
								list曲ルート_Dan.Add(c曲リストノード.list子リスト[i]);
								continue;
							}
						}
					}
                    else
					{
						for (int i = 0; i < c曲リストノード.list子リスト.Count; i++)
                        {
							if(c曲リストノード.list子リスト[i].arスコア[6] != null)
							{
								list曲ルート_Dan.Add(c曲リストノード.list子リスト[i]);

								if (TJAPlayer3.ConfigIni.bDanTowerHide)
									c曲リストノード.list子リスト.Remove(c曲リストノード.list子リスト[i]);
								
								continue;
							}
                        }
					}
				}
                else
				{
					// ???????

					/*
					if (c曲リストノード.arスコア[5] != null)
					{
						c曲リストノード.list子リスト.Remove(c曲リストノード);
						list曲ルート_Dan.Add(c曲リストノード);
						continue;
					}
					*/
				}
			}

			#region [ skin名で比較して、systemスキンとboxdefスキンに重複があれば、boxdefスキン側を削除する ]
			string[] systemSkinNames = CSkin.GetSkinName( TJAPlayer3.Skin.strSystemSkinSubfolders );
			List<string> l = new List<string>( listStrBoxDefSkinSubfolderFullName );
			foreach ( string boxdefSkinSubfolderFullName in l )
			{
				if ( Array.BinarySearch( systemSkinNames,
					CSkin.GetSkinName( boxdefSkinSubfolderFullName ),
					StringComparer.InvariantCultureIgnoreCase ) >= 0 )
				{
					listStrBoxDefSkinSubfolderFullName.Remove( boxdefSkinSubfolderFullName );
				}
			}
			#endregion
			string[] ba = listStrBoxDefSkinSubfolderFullName.ToArray();
			Array.Sort( ba );
			TJAPlayer3.Skin.strBoxDefSkinSubfolders = ba;
		}


		private void t曲リストへ後処理を適用する( List<C曲リストノード> ノードリスト, string parentName = "/", bool isGlobal = true )
		{
			
			if (isGlobal && ノードリスト.Count > 0)
            {
				var randomNode = CSongDict.tGenerateRandomButton(ノードリスト[0].r親ノード, parentName);
				ノードリスト.Add(randomNode);

			}


			// Don't sort songs if the folder isn't global
			// Call back reinsert back folders if sort called ?
			if (isGlobal)
			{
				#region [ Sort nodes ]
				//-----------------------------
				if (TJAPlayer3.ConfigIni.nDefaultSongSort == 0)
				{
					t曲リストのソート1_絶対パス順(ノードリスト);
				}
				else if (TJAPlayer3.ConfigIni.nDefaultSongSort == 1)
				{
					t曲リストのソート9_ジャンル順(ノードリスト, E楽器パート.TAIKO, 1, 0);
				}
				else if (TJAPlayer3.ConfigIni.nDefaultSongSort == 2)
				{
					t曲リストのソート9_ジャンル順(ノードリスト, E楽器パート.TAIKO, 2, 0);
				}
				//-----------------------------
				#endregion
			}

			// すべてのノードについて…
			foreach ( C曲リストノード c曲リストノード in ノードリスト )
			{
				SlowOrSuspendSearchTask();      // #27060 中断要求があったら、解除要求が来るまで待機, #PREMOVIE再生中は検索負荷を落とす

				#region [ Append "Back" buttons to the included folders ]
				//-----------------------------
				if ( c曲リストノード.eノード種別 == C曲リストノード.Eノード種別.BOX )
				{

					#region [ Sort child nodes ]
					//-----------------------------
					if (TJAPlayer3.ConfigIni.nDefaultSongSort == 0)
					{
						t曲リストのソート1_絶対パス順(c曲リストノード.list子リスト);
					}
					else if (TJAPlayer3.ConfigIni.nDefaultSongSort == 1)
					{
						t曲リストのソート9_ジャンル順(c曲リストノード.list子リスト, E楽器パート.TAIKO, 1, 0);
					}
					else if (TJAPlayer3.ConfigIni.nDefaultSongSort == 2)
					{
						t曲リストのソート9_ジャンル順(c曲リストノード.list子リスト, E楽器パート.TAIKO, 2, 0);
					}
					//-----------------------------
					#endregion


					string newPath = parentName + c曲リストノード.strタイトル + "/";

					CSongDict.tReinsertBackButtons(c曲リストノード, c曲リストノード.list子リスト, newPath, listStrBoxDefSkinSubfolderFullName);

					// Process subfolders recussively
					t曲リストへ後処理を適用する(c曲リストノード.list子リスト, newPath, false);

					continue;
				}

				//-----------------------------
				#endregion

				#region [ If no node title found, try to fetch it within the score objects ]
				//-----------------------------
				if ( string.IsNullOrEmpty( c曲リストノード.strタイトル ) )
				{
					for( int j = 0; j < (int)Difficulty.Total; j++ )
					{
						if( ( c曲リストノード.arスコア[ j ] != null ) && !string.IsNullOrEmpty( c曲リストノード.arスコア[ j ].譜面情報.タイトル ) )
						{
							c曲リストノード.strタイトル = c曲リストノード.arスコア[ j ].譜面情報.タイトル;

							if( TJAPlayer3.ConfigIni.bLog曲検索ログ出力 )
								Trace.TraceInformation( "タイトルを設定しました。(nID#{0:D3}, title={1})", c曲リストノード.nID, c曲リストノード.strタイトル );

							break;
						}
					}
				}
				//-----------------------------
				#endregion



			}

		}
		//-----------------
		#endregion		
		
		#region [ 曲リストソート ]
		//-----------------

	    public static void t曲リストのソート1_絶対パス順( List<C曲リストノード> ノードリスト )
	    {
	        t曲リストのソート1_絶対パス順(ノードリスト, E楽器パート.TAIKO, 1, 0);

	        foreach( C曲リストノード c曲リストノード in ノードリスト )
	        {
	            if( ( c曲リストノード.list子リスト != null ) && ( c曲リストノード.list子リスト.Count > 1 ) )
	            {
	                t曲リストのソート1_絶対パス順( c曲リストノード.list子リスト );
	            }
	        }
	    }

	    public static void t曲リストのソート1_絶対パス順( List<C曲リストノード> ノードリスト, E楽器パート part, int order, params object[] p )
	    {
            var comparer = new ComparerChain<C曲リストノード>(
                new C曲リストノードComparerノード種別(),
                new C曲リストノードComparer絶対パス(order),
                new C曲リストノードComparerタイトル(order));

	        ノードリスト.Sort( comparer );
	    }

	    public static void t曲リストのソート2_タイトル順( List<C曲リストノード> ノードリスト, E楽器パート part, int order, params object[] p )
	    {
	        var comparer = new ComparerChain<C曲リストノード>(
	            new C曲リストノードComparerノード種別(),
	            new C曲リストノードComparerタイトル(order),
	            new C曲リストノードComparer絶対パス(order));

	        ノードリスト.Sort( comparer );
	    }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ノードリスト"></param>
		/// <param name="part"></param>
		/// <param name="order">1=Ascend -1=Descend</param>
		public static void t曲リストのソート3_演奏回数の多い順( List<C曲リストノード> ノードリスト, E楽器パート part, int order, params object[] p )
		{
			order = -order;
			int nL12345 = (int) p[ 0 ];
			if ( part != E楽器パート.UNKNOWN )
			{
				ノードリスト.Sort( delegate( C曲リストノード n1, C曲リストノード n2 )
				{
					#region [ 共通処理 ]
					if( ( n1.eノード種別 == C曲リストノード.Eノード種別.BOX ) && ( n2.eノード種別 == C曲リストノード.Eノード種別.BOX ) )
					{
						return order * n1.arスコア[ 0 ].ファイル情報.フォルダの絶対パス.CompareTo( n2.arスコア[ 0 ].ファイル情報.フォルダの絶対パス );
					}
					#endregion
					int nSumPlayCountN1 = 0, nSumPlayCountN2 = 0;
//					for( int i = 0; i <(int)Difficulty.Total; i++ )
//					{
						if( n1.arスコア[ nL12345 ] != null )
						{
							nSumPlayCountN1 += n1.arスコア[ nL12345 ].譜面情報.演奏回数[ (int) part ];
						}
						if( n2.arスコア[ nL12345 ] != null )
						{
							nSumPlayCountN2 += n2.arスコア[ nL12345 ].譜面情報.演奏回数[ (int) part ];
						}
//					}
					var num = nSumPlayCountN2 - nSumPlayCountN1;
					if( num != 0 )
					{
						return order * num;
					}
					return order * n1.strタイトル.CompareTo( n2.strタイトル );
				} );
				foreach ( C曲リストノード c曲リストノード in ノードリスト )
				{
					int nSumPlayCountN1 = 0;
//					for ( int i = 0; i < 5; i++ )
//					{
						if ( c曲リストノード.arスコア[ nL12345 ] != null )
						{
							nSumPlayCountN1 += c曲リストノード.arスコア[ nL12345 ].譜面情報.演奏回数[ (int) part ];
						}
//					}
// Debug.WriteLine( nSumPlayCountN1 + ":" + c曲リストノード.strタイトル );
				}
			}
		}
		public static void t曲リストのソート4_LEVEL順( List<C曲リストノード> ノードリスト, E楽器パート part, int order, params object[] p )
		{
			order = -order;
			int nL12345 = (int)p[ 0 ];
			if ( part != E楽器パート.UNKNOWN )
			{
				ノードリスト.Sort( delegate( C曲リストノード n1, C曲リストノード n2 )
				{
					#region [ 共通処理 ]
					if ( ( n1.eノード種別 == C曲リストノード.Eノード種別.BOX ) && ( n2.eノード種別 == C曲リストノード.Eノード種別.BOX ) )
					{
						return order * n1.arスコア[ 0 ].ファイル情報.フォルダの絶対パス.CompareTo( n2.arスコア[ 0 ].ファイル情報.フォルダの絶対パス );
					}
					#endregion
					int nSumPlayCountN1 = 0, nSumPlayCountN2 = 0;
					if ( n1.arスコア[ nL12345 ] != null )
					{
						nSumPlayCountN1 = n1.nLevel[ nL12345 ];
					}
					if ( n2.arスコア[ nL12345 ] != null )
					{
						nSumPlayCountN2 = n2.nLevel[ nL12345 ];
					}
					var num = nSumPlayCountN2 - nSumPlayCountN1;
					if ( num != 0 )
					{
						return order * num;
					}
					return order * n1.strタイトル.CompareTo( n2.strタイトル );
				} );
				foreach ( C曲リストノード c曲リストノード in ノードリスト )
				{
					int nSumPlayCountN1 = 0;
					if ( c曲リストノード.arスコア[ nL12345 ] != null )
					{
						nSumPlayCountN1 = c曲リストノード.nLevel[ nL12345 ];
					}
// Debug.WriteLine( nSumPlayCountN1 + ":" + c曲リストノード.strタイトル );
				}
			}
		}
		public static void t曲リストのソート5_BestRank順( List<C曲リストノード> ノードリスト, E楽器パート part, int order, params object[] p )
		{
			order = -order;
			int nL12345 = (int) p[ 0 ];
			if ( part != E楽器パート.UNKNOWN )
			{
				ノードリスト.Sort( delegate( C曲リストノード n1, C曲リストノード n2 )
				{
					#region [ 共通処理 ]
					if ( ( n1.eノード種別 == C曲リストノード.Eノード種別.BOX ) && ( n2.eノード種別 == C曲リストノード.Eノード種別.BOX ) )
					{
						return order * n1.arスコア[ 0 ].ファイル情報.フォルダの絶対パス.CompareTo( n2.arスコア[ 0 ].ファイル情報.フォルダの絶対パス );
					}
					#endregion
					int nSumPlayCountN1 = 0, nSumPlayCountN2 = 0;
					bool isFullCombo1 = false, isFullCombo2 = false;
					if ( n1.arスコア[ nL12345 ] != null )
					{
						isFullCombo1 = n1.arスコア[ nL12345 ].譜面情報.フルコンボ[ (int) part ];
						nSumPlayCountN1 = n1.arスコア[ nL12345 ].譜面情報.最大ランク[ (int) part ];
					}
					if ( n2.arスコア[ nL12345 ] != null )
					{
						isFullCombo2 = n2.arスコア[ nL12345 ].譜面情報.フルコンボ[ (int) part ];
						nSumPlayCountN2 = n2.arスコア[ nL12345 ].譜面情報.最大ランク[ (int) part ];
					}
					if ( isFullCombo1 ^ isFullCombo2 )
					{
						if ( isFullCombo1 ) return order; else return -order;
					}
					var num = nSumPlayCountN2 - nSumPlayCountN1;
					if ( num != 0 )
					{
						return order * num;
					}
					return order * n1.strタイトル.CompareTo( n2.strタイトル );
				} );
				foreach ( C曲リストノード c曲リストノード in ノードリスト )
				{
					int nSumPlayCountN1 = 0;
					if ( c曲リストノード.arスコア[ nL12345 ] != null )
					{
						nSumPlayCountN1 = c曲リストノード.arスコア[ nL12345 ].譜面情報.最大ランク[ (int) part ];
					}
// Debug.WriteLine( nSumPlayCountN1 + ":" + c曲リストノード.strタイトル );
				}
			}
		}
		public static void t曲リストのソート6_SkillPoint順( List<C曲リストノード> ノードリスト, E楽器パート part, int order, params object[] p )
		{
			order = -order;
			int nL12345 = (int) p[ 0 ];
			if ( part != E楽器パート.UNKNOWN )
			{
				ノードリスト.Sort( delegate( C曲リストノード n1, C曲リストノード n2 )
				{
					#region [ 共通処理 ]
					if ( ( n1.eノード種別 == C曲リストノード.Eノード種別.BOX ) && ( n2.eノード種別 == C曲リストノード.Eノード種別.BOX ) )
					{
						return order * n1.arスコア[ 0 ].ファイル情報.フォルダの絶対パス.CompareTo( n2.arスコア[ 0 ].ファイル情報.フォルダの絶対パス );
					}
					#endregion
					double nSumPlayCountN1 = 0, nSumPlayCountN2 = 0;
					if ( n1.arスコア[ nL12345 ] != null )
					{
						nSumPlayCountN1 = n1.arスコア[ nL12345 ].譜面情報.最大スキル[ (int) part ];
					}
					if ( n2.arスコア[ nL12345 ] != null )
					{
						nSumPlayCountN2 = n2.arスコア[ nL12345 ].譜面情報.最大スキル[ (int) part ];
					}
					double d = nSumPlayCountN2 - nSumPlayCountN1;
					if ( d != 0 )
					{
						return order * System.Math.Sign(d);
					}
					return order * n1.strタイトル.CompareTo( n2.strタイトル );
				} );
				foreach ( C曲リストノード c曲リストノード in ノードリスト )
				{
					double nSumPlayCountN1 = 0;
					if ( c曲リストノード.arスコア[ nL12345 ] != null )
					{
						nSumPlayCountN1 = c曲リストノード.arスコア[ nL12345 ].譜面情報.最大スキル[ (int) part ];
					}
// Debug.WriteLine( nSumPlayCountN1 + ":" + c曲リストノード.strタイトル );
				}
			}
		}
		public static void t曲リストのソート7_更新日時順( List<C曲リストノード> ノードリスト, E楽器パート part, int order, params object[] p )
		{
			int nL12345 = (int) p[ 0 ];
			if ( part != E楽器パート.UNKNOWN )
			{
				ノードリスト.Sort( delegate( C曲リストノード n1, C曲リストノード n2 )
				{
					#region [ 共通処理 ]
					if ( ( n1.eノード種別 == C曲リストノード.Eノード種別.BOX ) && ( n2.eノード種別 == C曲リストノード.Eノード種別.BOX ) )
					{
						return order * n1.arスコア[ 0 ].ファイル情報.フォルダの絶対パス.CompareTo( n2.arスコア[ 0 ].ファイル情報.フォルダの絶対パス );
					}
					#endregion
					DateTime nSumPlayCountN1 = DateTime.Parse("0001/01/01 12:00:01.000");
					DateTime nSumPlayCountN2 = DateTime.Parse("0001/01/01 12:00:01.000");
					if ( n1.arスコア[ nL12345 ] != null )
					{
						nSumPlayCountN1 = n1.arスコア[ nL12345 ].ファイル情報.最終更新日時;
					}
					if ( n2.arスコア[ nL12345 ] != null )
					{
						nSumPlayCountN2 = n2.arスコア[ nL12345 ].ファイル情報.最終更新日時;
					}
					int d = nSumPlayCountN1.CompareTo(nSumPlayCountN2);
					if ( d != 0 )
					{
						return order * System.Math.Sign( d );
					}
					return order * n1.strタイトル.CompareTo( n2.strタイトル );
				} );
				foreach ( C曲リストノード c曲リストノード in ノードリスト )
				{
					DateTime nSumPlayCountN1 = DateTime.Parse( "0001/01/01 12:00:01.000" );
					if ( c曲リストノード.arスコア[ nL12345 ] != null )
					{
						nSumPlayCountN1 = c曲リストノード.arスコア[ nL12345 ].ファイル情報.最終更新日時;
					}
// Debug.WriteLine( nSumPlayCountN1 + ":" + c曲リストノード.strタイトル );
				}
			}
		}
		public static void t曲リストのソート8_アーティスト名順( List<C曲リストノード> ノードリスト, E楽器パート part, int order, params object[] p )
		{
			int nL12345 = (int) p[ 0 ]; 
			ノードリスト.Sort( delegate( C曲リストノード n1, C曲リストノード n2 )
			{
				string strAuthorN1 = "";
				string strAuthorN2 = "";
				if (n1.arスコア[ nL12345 ] != null ) {
					strAuthorN1 = n1.arスコア[ nL12345 ].譜面情報.アーティスト名;
				}
				if ( n2.arスコア[ nL12345 ] != null )
				{
					strAuthorN2 = n2.arスコア[ nL12345 ].譜面情報.アーティスト名;
				}

				return order * strAuthorN1.CompareTo( strAuthorN2 );
			} );
			foreach ( C曲リストノード c曲リストノード in ノードリスト )
			{
				string s = "";
				if ( c曲リストノード.arスコア[ nL12345 ] != null )
				{
					s = c曲リストノード.arスコア[ nL12345 ].譜面情報.アーティスト名;
				}
Debug.WriteLine( s + ":" + c曲リストノード.strタイトル );
			}
		}

	    public static void t曲リストのソート9_ジャンル順(List<C曲リストノード> ノードリスト, E楽器パート part, int order, params object[] p)
	    {
	        try
	        {
	            var acGenreComparer = order == 1
	                ? (IComparer<C曲リストノード>) new C曲リストノードComparerAC8_14()
	                : new C曲リストノードComparerAC15();

	            var comparer = new ComparerChain<C曲リストノード>(
	                new C曲リストノードComparerノード種別(),
	                acGenreComparer,
	                new C曲リストノードComparer絶対パス(1),
	                new C曲リストノードComparerタイトル(1));

	            ノードリスト.Sort( comparer );
	        }
	        catch (Exception ex)
	        {
	            Trace.TraceError(ex.ToString());
	            Trace.TraceError("例外が発生しましたが処理を継続します。 (bca6dda7-76ad-42fc-a415-250f52c0b17d)");
	        }
	    }
        //-----------------
        #endregion

        #region [ .score.ini を読み込んで Cスコア.譜面情報に設定する ]
        //-----------------
        public void tScoreIniを読み込んで譜面情報を設定する( string strScoreIniファイルパス, Cスコア score )
		{
			// New format
			string[] fp =
			{
				strScoreIniファイルパス + "1P.score.ini",
				strScoreIniファイルパス + "2P.score.ini",
			};

			// Load legacy format if new doesn't exist yet
			if (!File.Exists(fp[0]))
				fp[0] = strScoreIniファイルパス + ".score.ini";


			/*
			if ( !File.Exists( strScoreIniファイルパス ) )
				return;
			*/

			// Select the main file for the common informations
			int mainFile = 0;
			if (!File.Exists(fp[0]))
				mainFile = 1;
			if (!File.Exists(fp[1]) && mainFile == 1)
				return;

			// Only the necessary scores are read from the auxilliary score file
			int auxFile = mainFile ^ 1;

			try
			{
				//var ini = new CScoreIni( strScoreIniファイルパス );

				CScoreIni[] csi =
				{
					new CScoreIni(fp[mainFile]),
					File.Exists(fp[auxFile]) ? new CScoreIni(fp[auxFile]) : null,
				};

				var ini = csi[0];

				ini.t全演奏記録セクションの整合性をチェックし不整合があればリセットする();
				csi[1]?.t全演奏記録セクションの整合性をチェックし不整合があればリセットする();

				for ( int n楽器番号 = 0; n楽器番号 < 3; n楽器番号++ )
				{
					int n = ( n楽器番号 * 2 ) + 1;	// n = 0～5

					#region score.譜面情報.最大ランク[ n楽器番号 ] = ... 
					//-----------------
					if( ini.stセクション[ n ].b演奏にMIDI入力を使用した ||
						ini.stセクション[ n ].b演奏にキーボードを使用した ||
						ini.stセクション[ n ].b演奏にジョイパッドを使用した ||
						ini.stセクション[ n ].b演奏にマウスを使用した )
					{
						// (A) 全オートじゃないようなので、演奏結果情報を有効としてランクを算出する。

						score.譜面情報.最大ランク[ n楽器番号 ] =
							CScoreIni.tランク値を計算して返す( 
								ini.stセクション[ n ].n全チップ数,
								ini.stセクション[ n ].nPerfect数, 
								ini.stセクション[ n ].nGreat数,
								ini.stセクション[ n ].nGood数, 
								ini.stセクション[ n ].nPoor数,
								ini.stセクション[ n ].nMiss数 );
					}
					else
					{
						// (B) 全オートらしいので、ランクは無効とする。

						score.譜面情報.最大ランク[ n楽器番号 ] = (int) CScoreIni.ERANK.UNKNOWN;
					}
					//-----------------
					#endregion

					score.譜面情報.最大スキル[ n楽器番号 ] = ini.stセクション[ n ].db演奏型スキル値;
					score.譜面情報.フルコンボ[ n楽器番号 ] = ini.stセクション[ n ].bフルコンボである;
				}

				// Legacy
				score.譜面情報.ハイスコア = (int)ini.stセクション.HiScoreDrums.nスコア;
				score.譜面情報.nクリア = ini.stセクション.HiScoreDrums.nクリア;
				score.譜面情報.nスコアランク = ini.stセクション.HiScoreDrums.nスコアランク;

				for (int i = 0; i < (int)Difficulty.Total; i++)
				{
					score.譜面情報.nハイスコア[i] = (int)ini.stセクション.HiScoreDrums.nハイスコア[i];
				}

				// Load GPInfo for each save file
				for (int i = 0; i < 2; i++)
                {
					if (csi[i] == null)
						continue;

					score.GPInfo[i].nClear = csi[i].stセクション.HiScoreDrums.nクリア;
					score.GPInfo[i].nScoreRank = csi[i].stセクション.HiScoreDrums.nスコアランク;

					for (int j = 0; j < (int)Difficulty.Total; j++)
					{
						score.GPInfo[i].nHighScore[j] = (int)csi[i].stセクション.HiScoreDrums.nハイスコア[j];
					}
                }

				score.譜面情報.演奏回数.Drums = ini.stファイル.PlayCountDrums;
				score.譜面情報.演奏回数.Guitar = ini.stファイル.PlayCountGuitar;
				score.譜面情報.演奏回数.Bass = ini.stファイル.PlayCountBass;
				for( int i = 0; i < (int)Difficulty.Total; i++ )
					score.譜面情報.演奏履歴[ i ] = ini.stファイル.History[ i ];
			}
			catch (Exception e)
			{
				Trace.TraceError( "演奏記録ファイルの読み込みに失敗しました。[{0}]", strScoreIniファイルパス );
				Trace.TraceError( e.ToString() );
				Trace.TraceError( "例外が発生しましたが処理を継続します。 (801f823d-a952-4809-a1bb-cf6a56194f5c)" );
			}
		}
		//-----------------
		#endregion

		// その他


		#region [ private ]
		//-----------------
		//private const string SONGSDB_VERSION = "SongsDB5";
		private List<string> listStrBoxDefSkinSubfolderFullName;

		/// <summary>
		/// 検索を中断_スローダウンする
		/// </summary>
		private void SlowOrSuspendSearchTask()
		{
			if ( this.bIsSuspending )		// #27060 中断要求があったら、解除要求が来るまで待機
			{
				AutoReset.WaitOne();
			}
			if ( this.bIsSlowdown && ++this.searchCount > 10 )			// #27060 #PREMOVIE再生中は検索負荷を下げる
			{
				Thread.Sleep( 100 );
				this.searchCount = 0;
			}
		}

		//-----------------
		#endregion
	}
}
　
