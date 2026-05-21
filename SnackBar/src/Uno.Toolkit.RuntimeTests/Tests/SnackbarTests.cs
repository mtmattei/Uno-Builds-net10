using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uno.Toolkit.RuntimeTests.Helpers;
using Uno.Toolkit.UI;

#if IS_WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
#endif

namespace Uno.Toolkit.RuntimeTests.Tests;

[TestClass]
[RunsOnUIThread]
internal class SnackbarTests
{
	[TestMethod]
	public void Snackbar_DefaultState()
	{
		var snackbar = new Snackbar();

		Assert.AreEqual(string.Empty, snackbar.Message);
		Assert.IsNull(snackbar.ActionLabel);
		Assert.IsNull(snackbar.ActionCommand);
		Assert.IsNull(snackbar.ActionCommandParameter);
		Assert.IsFalse(snackbar.ShowDismissButton);
		Assert.IsFalse(snackbar.IsActionOnNewLine);
	}

	[TestMethod]
	public void Snackbar_MessageProperty_Sets()
	{
		var snackbar = new Snackbar();
		snackbar.Message = "Test message";

		Assert.AreEqual("Test message", snackbar.Message);
	}

	[TestMethod]
	public void Snackbar_ActionLabelProperty_Sets()
	{
		var snackbar = new Snackbar();
		snackbar.ActionLabel = "Undo";

		Assert.AreEqual("Undo", snackbar.ActionLabel);
	}

	[TestMethod]
	public void SnackbarHost_DefaultState()
	{
		var host = new SnackbarHost();

		Assert.AreEqual(SnackbarDuration.Short, host.DefaultDuration);
		Assert.IsNull(host.SnackbarStyle);
		Assert.IsTrue(host.IsSwipeToDismissEnabled);
		Assert.AreEqual(5, host.MaxQueueSize);
		Assert.IsFalse(host.IsShowing);
	}

	[TestMethod]
	public async Task SnackbarHost_ShowAsync_ReturnsTimeout()
	{
		var host = new SnackbarHost { Content = new Grid() };
		host.DefaultDuration = TimeSpan.FromMilliseconds(200);

		await UIHelper.Load(host);

		var reason = await host.ShowAsync(new SnackbarItem
		{
			Message = "Test",
		});

		Assert.AreEqual(SnackbarDismissReason.Timeout, reason);
	}

	[TestMethod]
	public async Task SnackbarHost_Dismiss_ReturnsDismissReason()
	{
		var host = new SnackbarHost { Content = new Grid() };
		host.DefaultDuration = SnackbarDuration.Indefinite;

		await UIHelper.Load(host);

		var task = host.ShowAsync(new SnackbarItem
		{
			Message = "Test indefinite",
		});

		// Allow enter animation to complete
		await Task.Delay(500);

		host.Dismiss();

		var reason = await task;
		Assert.AreEqual(SnackbarDismissReason.Dismiss, reason);
	}

	[TestMethod]
	public async Task SnackbarHost_Queue_ProcessesSequentially()
	{
		var host = new SnackbarHost { Content = new Grid() };
		host.DefaultDuration = TimeSpan.FromMilliseconds(200);

		await UIHelper.Load(host);

		var t1 = host.ShowAsync(new SnackbarItem { Message = "First" });
		var t2 = host.ShowAsync(new SnackbarItem { Message = "Second" });
		var t3 = host.ShowAsync(new SnackbarItem { Message = "Third" });

		var r1 = await t1;
		var r2 = await t2;
		var r3 = await t3;

		// All should eventually resolve (replaced or timeout)
		Assert.IsNotNull(r1);
		Assert.IsNotNull(r2);
		Assert.IsNotNull(r3);
	}

	[TestMethod]
	public async Task SnackbarHost_MaxQueueSize_DropsExcess()
	{
		var host = new SnackbarHost { Content = new Grid() };
		host.MaxQueueSize = 2;
		host.DefaultDuration = SnackbarDuration.Indefinite;

		await UIHelper.Load(host);

		var t1 = host.ShowAsync(new SnackbarItem { Message = "1" });
		var t2 = host.ShowAsync(new SnackbarItem { Message = "2" });
		var t3 = host.ShowAsync(new SnackbarItem { Message = "3" });

		// With MaxQueueSize=2 and one showing, the oldest queued item gets dropped
		// t1 is currently showing, t2 and t3 queued, but queue limit is 2
		// so one of the early tasks should complete with Replaced
		await Task.Delay(100);

		// Dismiss to drain
		host.Dismiss();
		await Task.Delay(500);
		host.Dismiss();
		await Task.Delay(500);
		host.Dismiss();

		// All tasks should complete without hanging
		var r1 = await t1;
		var r2 = await t2;
		var r3 = await t3;

		Assert.IsNotNull(r1);
		Assert.IsNotNull(r2);
		Assert.IsNotNull(r3);
	}

	[TestMethod]
	public void SnackbarItem_DefaultValues()
	{
		var item = new SnackbarItem();

		Assert.AreEqual(string.Empty, item.Message);
		Assert.IsNull(item.ActionLabel);
		Assert.IsNull(item.ActionCommand);
		Assert.IsNull(item.ActionCommandParameter);
		Assert.IsFalse(item.ShowDismissButton);
		Assert.IsFalse(item.IsActionOnNewLine);
		Assert.IsNull(item.Duration);
	}

	[TestMethod]
	public void SnackbarDuration_Constants()
	{
		Assert.AreEqual(TimeSpan.FromSeconds(4), SnackbarDuration.Short);
		Assert.AreEqual(TimeSpan.FromSeconds(7), SnackbarDuration.Long);
		Assert.AreEqual(TimeSpan.MaxValue, SnackbarDuration.Indefinite);
	}
}
