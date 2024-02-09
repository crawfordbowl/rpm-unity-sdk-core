using System.Collections.Generic;
using System.Threading.Tasks;
using ReadyPlayerMe.AvatarCreator;
using ReadyPlayerMe.Core;
using UnityEngine;
using UnityEngine.Events;

#pragma warning disable CS4014
#pragma warning disable CS1998

namespace ReadyPlayerMe.Samples.AvatarCreatorExperimental
{

    /// <summary>
    /// A class responsible for creating and customizing avatars using asset and color selections.
    /// </summary>
    public class SimpleAvatarCreator : MonoBehaviour
    {
        public UnityEvent<AvatarProperties> onAvatarCreated;
        [SerializeField] private List<AssetSelectionElement> assetSelectionElements;
        [SerializeField] private List<ColorSelectionElement> colorSelectionElements;
        [SerializeField] private RuntimeAnimatorController animationController;
        [SerializeField] private GameObject loading;

        [SerializeField] private BodyType bodyType = BodyType.FullBody;
        private readonly OutfitGender gender = OutfitGender.Masculine;

        private AvatarManager avatarManager;
        private GameObject avatar;

        /// <summary>
        /// Start is used to initialize the avatar creator and loads initial avatar assets.
        /// </summary>
        private async void Start()
        {
            await AuthManager.LoginAsAnonymous();
            avatarManager = new AvatarManager();

            loading.SetActive(true);
            LoadAssets();
            var avatarProperties = await CreateTemplateAvatar();
            GetColors(avatarProperties);
            loading.SetActive(false);
        }

        private void OnEnable()
        {
            // Subscribes to asset selection events when this component is enabled.
            foreach (var element in assetSelectionElements)
            {
                element.OnAssetSelected.AddListener(OnAssetSelection);
            }

            foreach (var element in colorSelectionElements)
            {
                element.OnAssetSelected.AddListener(OnAssetSelection);
            }
        }

        private void OnDisable()
        {
            // Unsubscribes from asset selection events when this component is disabled.
            foreach (var element in assetSelectionElements)
            {
                element.OnAssetSelected.RemoveListener(OnAssetSelection);
            }

            foreach (var element in colorSelectionElements)
            {
                element.OnAssetSelected.RemoveListener(OnAssetSelection);
            }
        }

        /// <summary>
        /// Handles the selection of an asset and updates the avatar accordingly.
        /// </summary>
        /// <param name="assetData">The selected asset data.</param>
        private async void OnAssetSelection(IAssetData assetData)
        {
            loading.SetActive(true);
            var newAvatar = await avatarManager.UpdateAsset(assetData.AssetType, bodyType, assetData.Id);

            // Destroy the old avatar and replace it with the new one.
            if (avatar != null)
            {
                Destroy(avatar);
            }
            avatar = newAvatar;
            SetupAvatar();
            loading.SetActive(false);
        }

        /// <summary>
        /// Loads and initializes asset selection elements for avatar customization.
        /// </summary>
        private async void LoadAssets()
        {
            foreach (var element in assetSelectionElements)
            {
                element.LoadAndCreateButtons(gender);
            }
        }

        /// <summary>
        /// Loads and initializes color selection elements for choosing avatar colors.
        /// </summary>
        /// <param name="avatarProperties">The properties of the avatar.</param>
        private void GetColors(AvatarProperties avatarProperties)
        {
            foreach (var element in colorSelectionElements)
            {
                element.LoadAndCreateButtons(avatarProperties);
            }
        }

        /// <summary>
        /// Creates an avatar from a template and sets its initial properties.
        /// </summary>
        /// <returns>The properties of the created avatar.</returns>
        private async Task<AvatarProperties> CreateTemplateAvatar()
        {
            var avatarTemplateFetcher = new AvatarTemplateFetcher();
            var templates = await avatarTemplateFetcher.GetTemplates();
            var avatarTemplate = templates[1];

            var templateAvatarProps = await avatarManager.CreateAvatarFromTemplate(avatarTemplate.Id, bodyType);
            avatar = templateAvatarProps.Item1;
            SetupAvatar();
            onAvatarCreated?.Invoke(templateAvatarProps.Item2);
            return templateAvatarProps.Item2;
        }

        /// <summary>
        /// Sets additional elements and components on the created avatar, such as mouse rotation and animation controller.
        /// </summary>
        private void SetupAvatar()
        {
            avatar.AddComponent<MouseRotationHandler>();
            avatar.AddComponent<AvatarRotator>();
            avatar.GetComponent<Animator>().runtimeAnimatorController = animationController;
        }
    }
}
