using Puppeteer.Core.Debug;
using System;

#if UNITY_EDITOR
using UnityEditor.VersionControl;
#endif

namespace Puppeteer.Core.Helper
{
	public static class VersionControl
	{
		[Flags]
		public enum Result : ushort
		{
			None = 0,
			Controlled = 1,
			NotControlled = 2,
			Fail = 4,
			Invalid = 8,
			Locked = 16
		}

#if UNITY_EDITOR
		public static Result AddAsset(Asset _asset)
		{
			Provider.Add(_asset, true).Wait();
			return Result.Controlled;
		}

		public static Result CheckOutAsset(Asset _asset)
		{
			if (_asset.IsOneOfStates(OUTDATED))
			{
				PuppeteerLogger.Log(string.Format("[Puppeteer Version Control] The asset you are overriding" +
										 "is outdated. asset: {0}", _asset.name), LogType.Warning);
			}

			if (_asset.IsOneOfStates(USED_REMOTE))
			{
				PuppeteerLogger.Log(string.Format("[Puppeteer Version Control] The asset you are overriding " +
										 "is being modified by another user. asset: {0}", _asset.name), LogType.Warning);
			}

			if (_asset.IsOneOfStates(LOCKED))
			{
				PuppeteerLogger.Log(string.Format("[Puppeteer Version Control] The asset you are overriding " +
										 "is locked. asset: {0}", _asset.name), LogType.Warning);
				return Result.Locked;
			}

			Provider.Checkout(_asset, CheckoutMode.Asset).Wait();

			return Result.Controlled;
		}

		public static Result IsUnderControl(string _path, out Asset _asset)
		{
			if (!Provider.isActive)
			{
				_asset = null;
				return Result.Fail;
			}

			_asset = Provider.GetAssetByPath(_path.RelativePath());
			if (_asset is null)
			{
				return Result.Invalid;
			}

			do
			{
				Provider.Status(_asset).Wait();
			} while (_asset.IsState(Asset.States.Updating));

			return Provider.CheckoutIsValid(_asset) ? Result.Controlled : Result.NotControlled;
		}
#endif


		public static bool IsUsable(Result _result)
		{
			return !((_result & (Result.Fail | Result.Invalid | Result.Locked)) != 0);
		}

		public static Result TryCheckOutOrAdd(string _path)
		{
#if UNITY_EDITOR
			Result assetState = IsUnderControl(_path, out Asset asset);
			if (assetState == Result.NotControlled && asset != null)
			{
				return AddAsset(asset);
			}
			else if (assetState == Result.Controlled && !Provider.IsOpenForEdit(asset))
			{
				return CheckOutAsset(asset);
			}
#endif
			return Result.NotControlled;
		}

#if UNITY_EDITOR
		private static readonly Asset.States[] LOCKED = { Asset.States.LockedRemote };
		private static readonly Asset.States[] OUTDATED = { Asset.States.OutOfSync, Asset.States.Conflicted };
		private static readonly Asset.States[] USED_REMOTE = { Asset.States.CheckedOutRemote, Asset.States.DeletedRemote, Asset.States.LockedRemote, Asset.States.MovedRemote };
#endif

	}
}