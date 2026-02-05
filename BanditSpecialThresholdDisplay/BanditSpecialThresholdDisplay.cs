using BepInEx;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace BanditSpecialThresholdDisplay {

  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class BanditSpecialThresholdDisplay : BaseUnityPlugin {

    public const string PluginGUID = PluginAuthor + "." + PluginName;
    public const string PluginAuthor = "Ditpowuh";
    public const string PluginName = "BanditSpecialThresholdDisplay";
    public const string PluginVersion = "0.1.0";

    public static CharacterBody localBanditBody;

    public void Awake() {
      Log.Init(Logger);

      On.RoR2.CharacterBody.Start += CharacterBody_Start;
      On.RoR2.UI.HealthBar.Awake += HealthBar_Awake;
    }

    void CharacterBody_Start(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self) {
      orig(self);

      if (!self.isPlayerControlled) {
        return;
      }

      BodyIndex banditIndex = BodyCatalog.FindBodyIndex("Bandit2Body");
      if (banditIndex == self.bodyIndex) {
        localBanditBody = self;
      }
    }

    void HealthBar_Awake(On.RoR2.UI.HealthBar.orig_Awake orig, HealthBar self) {
      orig(self);

      if (!BanditSpecialThresholdDisplay.localBanditBody) {
        return;
      }

      Image originalImage = self.transform.GetComponentInChildren<Image>();
      if (originalImage == null) {
        return;
      }

      RectTransform originalRect = originalImage.GetComponent<RectTransform>();

      GameObject overlayGameObject = new GameObject("CustomOverlayBar");
      overlayGameObject.transform.SetParent(self.transform, false);
      overlayGameObject.transform.SetAsLastSibling();

      RectTransform overlayRect = overlayGameObject.AddComponent<RectTransform>();
      overlayRect.sizeDelta = originalRect.sizeDelta;
      overlayRect.pivot = originalRect.pivot;
      overlayRect.anchorMin = originalRect.anchorMin;
      overlayRect.anchorMax = originalRect.anchorMax;
      overlayRect.anchoredPosition = originalRect.anchoredPosition;

      Image overlayImage = overlayGameObject.AddComponent<Image>();
      overlayImage.color = new Color(1f, 0f, 1f, 0.25f);
      overlayImage.sprite = originalImage.sprite;
      overlayImage.preserveAspect = originalImage.preserveAspect;
      overlayImage.type = Image.Type.Filled;
      overlayImage.fillMethod = Image.FillMethod.Horizontal;
      overlayImage.fillOrigin = originalImage.fillOrigin;

      overlayGameObject.AddComponent<CustomBarUpdater>().Init(overlayImage, self);
    }

  }

  public class CustomBarUpdater : MonoBehaviour {
    Image customBar;
    HealthBar healthBar;

    public void Init(Image img, HealthBar bar) {
      customBar = img;
      healthBar = bar;
    }

    void Update() {
      if (!healthBar || !healthBar.source) {
        customBar.fillAmount = 0f;
        return;
      }

      CharacterBody enemyBody = healthBar.source.body;
      if (!enemyBody || !enemyBody.healthComponent) {
        customBar.fillAmount = 0f;
        return;
      }

      float enemyMaxHp = enemyBody.healthComponent.fullHealth;
      if (enemyMaxHp <= 0f) {
        customBar.fillAmount = 0f;
        return;
      }

      float banditDamage = BanditSpecialThresholdDisplay.localBanditBody.damage;
      int banditDesperadoBuff = BanditSpecialThresholdDisplay.localBanditBody.GetBuffCount(RoR2Content.Buffs.BanditSkull);

      float fill = (banditDamage * 6f * (1f + banditDesperadoBuff * 0.1f)) / enemyMaxHp + 0.025f;
      customBar.fillAmount = Mathf.Clamp01(fill);
    }
  }

}
