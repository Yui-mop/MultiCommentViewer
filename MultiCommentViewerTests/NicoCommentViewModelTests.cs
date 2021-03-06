﻿using Common;
using Moq;
using MultiCommentViewer;
using NicoSitePlugin;
using NUnit.Framework;
using Plugin;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiCommentViewerTests
{
    [TestFixture]
    class NicoCommentViewModelTests
    {
        [Test]
        public async Task Test()
        {
            var userId = "1";
            var user = new UserTest(userId)
            {
                 Name= new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText("name1") },
            };
            var serverMock = new Mock<IDataSource>();
            var server = serverMock.Object;
            var loggerMock = new Mock<ILogger>();
            var logger = loggerMock.Object;
            var optionsMock = new Mock<IOptions>();
            var options = optionsMock.Object;

            var metadataMock = new Mock<IMessageMetadata>();
            metadataMock.Setup(m => m.User).Returns(user);
            var methodsMock = new Mock<IMessageMethods>();

            var metadata = metadataMock.Object;
            var methods = methodsMock.Object;
            var connectionName = new ConnectionName();
            var connectionStatus = new Mock<IConnectionStatus>().Object;
            var siteOptionsMock = new Mock<INicoSiteOptions>();
            siteOptionsMock.Setup(s => s.IsAutoSetNickname).Returns(true);
            var siteOptions = siteOptionsMock.Object;

            var userInfoMock = new Mock<IUserInfo>();
            var userInfo = userInfoMock.Object;

            var chat1 = new Chat("<chat thread=\"1645724171\" no=\"4\" vpos=\"180000\" date=\"1550890471\" date_usec=\"549074\" mail=\"184\" user_id=\"G-lRat9seQmpK-gcgcQXSFxr14c\" premium=\"1\" anonymity=\"1\" locale=\"ja-jp\">message1</chat>");
            var comment1 = await Tools.CreateNicoComment(chat1, user,siteOptions,"",userid=> Task.FromResult(userInfo),logger) as INicoComment;
            var cvm1 = new NicoCommentViewModel(comment1, metadata, methods, connectionStatus, options);
            Assert.IsNull(cvm1.NameItems);

            var chat2 = new Chat("<chat thread=\"1645724171\" no=\"4\" vpos=\"180000\" date=\"1550890471\" date_usec=\"549074\" mail=\"184\" user_id=\"G-lRat9seQmpK-gcgcQXSFxr14c\" premium=\"1\" anonymity=\"1\" locale=\"ja-jp\">message2@newnick</chat>");
            var comment2 = await Tools.CreateNicoComment(chat2, user, siteOptions, "", userid => Task.FromResult(userInfo), logger) as INicoComment;
            var cvm2 = new NicoCommentViewModel(comment2, metadata, methods, connectionStatus, options);
            Assert.AreEqual(new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText("newnick") }, cvm1.NameItems);
            Assert.AreEqual(new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText("newnick") }, cvm2.NameItems);
        }
        [Test]
        public void ItemTest()
        {
            var itemMock = new Mock<INicoItem>();
            var item = itemMock.Object;
            var metadataMock = new Mock<IMessageMetadata>();
            var metadata = metadataMock.Object;
            var methodsMock = new Mock<IMessageMethods>();
            var methods = methodsMock.Object;
            var connectionStatusMock = new Mock<IConnectionStatus>();
            var connectionStatus = connectionStatusMock.Object;
            var optionsMock = new Mock<IOptions>();
            var options = optionsMock.Object;
            var cvm = new NicoCommentViewModel(item, metadata, methods, connectionStatus, options);

        }
        [Test]
        public void IsVisibleTest()
        {
            var messageMock = new Mock<INicoComment>();
            messageMock.Setup(m => m.Id).Returns("");
            var metadataMock = new Mock<IMessageMetadata>();
            var methodsMock = new Mock<IMessageMethods>();
            var connectionStatusMock = new Mock<IConnectionStatus>();
            var optionsMock = new Mock<IOptions>();

            var message = messageMock.Object;
            var metadata = metadataMock.Object;
            var methods = methodsMock.Object;
            var connectionStatus = connectionStatusMock.Object;
            var options = optionsMock.Object;

            var cvm = new NicoCommentViewModel(message, metadata, methods, connectionStatus, options);
            var isVisible = false;
            cvm.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(metadata.IsVisible):
                        isVisible = cvm.IsVisible;
                        break;
                }
            };
            metadataMock.Setup(u => u.IsVisible).Returns(true);
            metadataMock.Raise(c => c.PropertyChanged += null, new PropertyChangedEventArgs(nameof(metadata.IsVisible)));
            Assert.IsTrue(isVisible);
        }
        [Test]
        public void IsInvisibleTest()
        {
            var messageMock = new Mock<INicoComment>();
            messageMock.Setup(m => m.Id).Returns("");
            var metadataMock = new Mock<IMessageMetadata>();
            var methodsMock = new Mock<IMessageMethods>();
            var connectionStatusMock = new Mock<IConnectionStatus>();
            var optionsMock = new Mock<IOptions>();

            var message = messageMock.Object;
            var metadata = metadataMock.Object;
            var methods = methodsMock.Object;
            var connectionStatus = connectionStatusMock.Object;
            var options = optionsMock.Object;

            var cvm = new NicoCommentViewModel(message, metadata, methods, connectionStatus, options);
            var isVisible = true;
            cvm.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(metadata.IsVisible):
                        isVisible = cvm.IsVisible;
                        break;
                }
            };
            metadataMock.Setup(u => u.IsVisible).Returns(false);
            metadataMock.Raise(c => c.PropertyChanged += null, new PropertyChangedEventArgs(nameof(metadata.IsVisible)));
            Assert.IsFalse(isVisible);
        }
    }
}
