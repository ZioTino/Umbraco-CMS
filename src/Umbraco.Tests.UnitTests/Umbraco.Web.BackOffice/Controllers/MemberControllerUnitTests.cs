using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Events;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Security;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Infrastructure.Security;
using Umbraco.Tests.UnitTests.AutoFixture;
using Umbraco.Tests.UnitTests.Umbraco.Core.ShortStringHelper;
using Umbraco.Web;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.Common.Exceptions;
using Umbraco.Web.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.BackOffice.Controllers
{
    [TestFixture]
    public class MemberControllerUnitTests
    {
        [Test]
        [AutoMoqData]
        public void PostSaveMember_WhenMemberIsNull_ExpectFailureResponse(
            MemberController sut)
        {
            // arrange
            // act
            ArgumentNullException exception = Assert.ThrowsAsync<ArgumentNullException>(() => sut.PostSave(null));

            // assert
            Assert.That(exception.Message, Is.EqualTo("Value cannot be null. (Parameter 'The member content item was null')"));
        }

        [Test]
        [AutoMoqData]
        public void PostSaveMember_WhenModelStateIsNotValid_ExpectFailureResponse(
            [Frozen] IMembersUserManager umbracoMembersUserManager,
            IMemberTypeService memberTypeService,
            IDataTypeService dataTypeService,
            IMemberService memberService,
            MapDefinitionCollection memberMapDefinition,
            PropertyEditorCollection propertyEditorCollection,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            IBackOfficeSecurity backOfficeSecurity)
        {
            // arrange
            Member member = SetupMemberTestData(memberMapDefinition, out UmbracoMapper mapper, out MemberSave fakeMemberData, out MemberDisplay memberDisplay, ContentSaveAction.SaveNew);
            //Mock.Get(umbracoMembersUserManager)
            //    .Setup(x => x.CreateAsync(It.IsAny<MembersIdentityUser>(), It.IsAny<string>()))
            //    .ReturnsAsync(() => IdentityResult.Success);
            //Mock.Get(umbracoMembersUserManager)
            //    .Setup(x => x.ValidatePasswordAsync(It.IsAny<string>()))
            //    .ReturnsAsync(() => IdentityResult.Success);
            //Mock.Get(memberTypeService).Setup(x => x.GetDefault()).Returns("fakeAlias");
            //Mock.Get(backOfficeSecurityAccessor).Setup(x => x.BackOfficeSecurity).Returns(backOfficeSecurity);
            ////Mock.Get(memberService).SetupSequence(
            //        x => x.GetByEmail(It.IsAny<string>()))
            //    .Returns(() => null)
            //    .Returns(() => member);

            MemberController sut = CreateSut(mapper, memberService, memberTypeService, umbracoMembersUserManager, dataTypeService, propertyEditorCollection, backOfficeSecurityAccessor);
            sut.ModelState.AddModelError("key", "Invalid model state");


            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.CreateAsync(It.IsAny<MembersIdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(() => IdentityResult.Success);
            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.ValidatePasswordAsync(It.IsAny<string>()))
                .ReturnsAsync(() => IdentityResult.Success);

            var value = new MemberDisplay();
            string reason = "Validation failed";

            // act
            HttpResponseException exception = Assert.ThrowsAsync<HttpResponseException>(() => sut.PostSave(fakeMemberData));

            // assert
            AssertExpectedException(exception, value, reason);
        }


        [Test]
        [AutoMoqData]
        public async Task PostSaveMember_SaveNew_WhenAllIsSetupCorrectly_ExpectSuccessResponse(
            [Frozen] IMembersUserManager umbracoMembersUserManager,
            IMemberTypeService memberTypeService,
            IDataTypeService dataTypeService,
            IMemberService memberService,
            MapDefinitionCollection memberMapDefinition,
            PropertyEditorCollection propertyEditorCollection,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            IBackOfficeSecurity backOfficeSecurity)
        {
            // arrange
            Member member = SetupMemberTestData(memberMapDefinition, out UmbracoMapper mapper, out MemberSave fakeMemberData, out MemberDisplay memberDisplay, ContentSaveAction.SaveNew);
            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.CreateAsync(It.IsAny<MembersIdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(() => IdentityResult.Success);
            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.ValidatePasswordAsync(It.IsAny<string>()))
                .ReturnsAsync(() => IdentityResult.Success);
            Mock.Get(memberTypeService).Setup(x => x.GetDefault()).Returns("fakeAlias");
            Mock.Get(backOfficeSecurityAccessor).Setup(x => x.BackOfficeSecurity).Returns(backOfficeSecurity);
            Mock.Get(memberService).SetupSequence(
                x => x.GetByEmail(It.IsAny<string>()))
                .Returns(() => null)
                .Returns(() => member);

            MemberController sut = CreateSut(mapper, memberService, memberTypeService, umbracoMembersUserManager, dataTypeService, propertyEditorCollection, backOfficeSecurityAccessor);

            // act
            ActionResult<MemberDisplay> actualResult = await sut.PostSave(fakeMemberData);

            // assert
            Assert.AreEqual(memberDisplay, actualResult.Value);
        }

        [Test]
        [AutoMoqData]
        public async Task PostSaveMember_Save_WhenAllIsSetupCorrectly_ExpectSuccessResponse(
            [Frozen] IMembersUserManager umbracoMembersUserManager,
            IMemberTypeService memberTypeService,
            IDataTypeService dataTypeService,
            IMemberService memberService,
            MapDefinitionCollection memberMapDefinition,
            PropertyEditorCollection propertyEditorCollection,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            IBackOfficeSecurity backOfficeSecurity)
        {
            // arrange
            Member member = SetupMemberTestData(memberMapDefinition, out UmbracoMapper mapper, out MemberSave fakeMemberData, out MemberDisplay memberDisplay, ContentSaveAction.Save);
            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(() => new MembersIdentityUser());
            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.ValidatePasswordAsync(It.IsAny<string>()))
                .ReturnsAsync(() => IdentityResult.Success);

            string password = "fakepassword9aw89rnyco3938cyr^%&*()i8Y";
            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.GeneratePassword(It.IsAny<string>()))
                .Returns(password);
            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.UpdateAsync(It.IsAny<MembersIdentityUser>()))
                .ReturnsAsync(() => IdentityResult.Success);
            Mock.Get(memberTypeService).Setup(x => x.GetDefault()).Returns("fakeAlias");
            Mock.Get(backOfficeSecurityAccessor).Setup(x => x.BackOfficeSecurity).Returns(backOfficeSecurity);

            Mock.Get(memberService).SetupSequence(
                    x => x.GetByEmail(It.IsAny<string>()))
                .Returns(() => null)
                .Returns(() => member);

            MemberController sut = CreateSut(mapper, memberService, memberTypeService, umbracoMembersUserManager, dataTypeService, propertyEditorCollection, backOfficeSecurityAccessor);

            // act
            ActionResult<MemberDisplay> actualResult = await sut.PostSave(fakeMemberData);

            // assert
            Assert.AreEqual(memberDisplay, actualResult.Value);
        }

        [Test]
        [AutoMoqData]
        public void PostSaveMember_SaveNew_WhenMemberEmailAlreadyExists_ExpectSuccessResponse(
            [Frozen] IMembersUserManager umbracoMembersUserManager,
            IMemberTypeService memberTypeService,
            IDataTypeService dataTypeService,
            IMemberService memberService,
            MapDefinitionCollection memberMapDefinition,
            PropertyEditorCollection propertyEditorCollection,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            IBackOfficeSecurity backOfficeSecurity)
        {
            // arrange
            Member member = SetupMemberTestData(memberMapDefinition, out UmbracoMapper mapper, out MemberSave fakeMemberData, out MemberDisplay memberDisplay, ContentSaveAction.SaveNew);
            Mock.Get(umbracoMembersUserManager)
                .Setup(x => x.CreateAsync(It.IsAny<MembersIdentityUser>()))
                .ReturnsAsync(() => IdentityResult.Success);
            Mock.Get(memberTypeService).Setup(x => x.GetDefault()).Returns("fakeAlias");
            Mock.Get(backOfficeSecurityAccessor).Setup(x => x.BackOfficeSecurity).Returns(backOfficeSecurity);

            Mock.Get(memberService).SetupSequence(
                    x => x.GetByEmail(It.IsAny<string>()))
                .Returns(() => member);

            MemberController sut = CreateSut(mapper, memberService, memberTypeService, umbracoMembersUserManager, dataTypeService, propertyEditorCollection, backOfficeSecurityAccessor);
            var value = new MemberDisplay();
            string reason = "Validation failed";

            // act
            HttpResponseException exception = Assert.ThrowsAsync<HttpResponseException>(() => sut.PostSave(fakeMemberData));

            // assert
            AssertExpectedException(exception, value, reason);
        }

        private void AssertExpectedException(HttpResponseException exception, object value, string reason)
        {
            var expectedException = new HttpResponseException(HttpStatusCode.BadRequest, value)
            {
                AdditionalHeaders =
                {
                    ["X-Status-Reason"] = reason
                }
            };

            Assert.That(exception.AdditionalHeaders, Is.EqualTo(expectedException.AdditionalHeaders));
            Assert.That(exception.Value, Is.EqualTo(expectedException.Value));
            Assert.That(exception.Status, Is.EqualTo(expectedException.Status));
        }

        /// <summary>
        /// Setup all standard member data for test
        /// </summary>
        private Member SetupMemberTestData(
            MapDefinitionCollection memberMapDefinition,
            out UmbracoMapper mapper,
            out MemberSave fakeMemberData,
            out MemberDisplay memberDisplay,
            ContentSaveAction contentAction)
        {
            var memberType = new MemberType(new DefaultShortStringHelper(new DefaultShortStringHelperConfig()), int.MinValue);
            IMemberType testContentType = memberType;

            string fakePassword = "i2ruf38vrba8^&T^";
            var testName = "Test Name";
            var testEmail = "test@umbraco.com";
            var testUser = "TestUser";

            var member = new Member(testName, testEmail, testUser, testContentType) { RawPasswordValue = fakePassword };
            mapper = new UmbracoMapper(memberMapDefinition);

            // TODO: reuse maps
            mapper.Define<Member, MemberDisplay>((m, context) => new MemberDisplay()
            {
                Username = m.Username
            });
            mapper.Define<MemberSave, IMember>((m, context) => new Member(new Mock<IMemberType>().Object));
            fakeMemberData = CreateFakeMemberData(member, contentAction);

            memberDisplay = new MemberDisplay()
            {
            };

            return member;
        }

        private MemberController CreateSut(
            UmbracoMapper mapper,
            IMemberService memberService,
            IMemberTypeService memberTypeService,
            IMembersUserManager membersUserManager,
            IDataTypeService dataTypeService,
            PropertyEditorCollection propertyEditorCollection,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor) =>
            new MemberController(
                new DefaultCultureDictionary(
                    new Mock<ILocalizationService>().Object,
                    new HttpRequestAppCache(() => null)),
                new LoggerFactory(),
                new MockShortStringHelper(),
                new DefaultEventMessagesFactory(
                    new Mock<IEventMessagesAccessor>().Object),
                new Mock<ILocalizedTextService>().Object,
                propertyEditorCollection,
                mapper,
                memberService,
                memberTypeService,
                membersUserManager,
                dataTypeService,
                backOfficeSecurityAccessor,
                new ConfigurationEditorJsonSerializer());

        private static MemberSave CreateFakeMemberData(IMember member, ContentSaveAction action)
        {
            var fakeMemberData = new MemberSave()
            {
                Id = 123,
                Password = new ChangingPasswordModel()
                {
                    Id = 123,
                    NewPassword = member.RawPasswordValue,
                    OldPassword = null
                },
                Name = member.Name,
                Email = member.Email,
                Username = member.Username,
                PersistedContent = member,
                PropertyCollectionDto = new ContentPropertyCollectionDto()
                {
                },
                Groups = new List<string>(),
                Alias = "fakeAlias",
                ContentTypeAlias = "fakeContentType",
                Action = action
            };
            return fakeMemberData;
        }
    }
}
