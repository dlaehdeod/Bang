using UnityEngine;

public class CardTransform
{
    public static CardTransform instance = null;

    public Transform[] playCard;
    public Transform[] characterCard;
    public Transform[] jobCard;

    public CardTransform ()
    {
        instance = this;
    }

    public void ResetPlayCard (Transform parent)
    {
        int layer = LayerMask.NameToLayer("Show Card");
        for (int i = 0; i < playCard.Length; ++i)
        {
            playCard[i].gameObject.layer = layer;
            playCard[i].GetChild(0).gameObject.SetActive(true);
        }

        ResetCard(parent, playCard);
    }

    public void ResetCharacterCard (Transform parent)
    {
        ResetCard(parent, characterCard);
    }

    public void ResetJobCard (Transform parent)
    {
        for (int i = 0; i < jobCard.Length; ++i)
        {
            jobCard[i].GetChild(0).gameObject.SetActive(true);
        }

        ResetCard(parent, jobCard);
    }

    private void ResetCard (Transform parent, Transform[] card)
    {
        foreach (Transform c in card)
        {
            c.SetParent(parent);
            c.localPosition = Vector3.zero;
            c.localRotation = Quaternion.identity;
        }
    }

    public int FindPlayCard (Transform target)
    {
        for (int i = 0; i < playCard.Length; ++i)
        {
            if (playCard[i] == target)
            {
                return i;
            }
        }

        return -1;
    }

    public void SetPlayCard (Transform playCardParent)
    {
        int length = playCardParent.childCount;
        playCard = new Transform[length];

        for (int i = 0; i < length; ++i)
        {
            playCard[i] = FindTarget((Card)i, playCardParent);
        }
    }

    private Transform FindTarget (Card card, Transform parent)
    {
        string cardName = card.ToString();

        for (int i = 0; i < parent.childCount; ++i)
        {
            if (parent.GetChild(i).name.Equals(cardName))
            {
                return parent.GetChild(i);
            }
        }

        return null;
    }

    public void SetCharacterCard (Transform characterCardParent)
    {
        int length = characterCardParent.childCount;
        characterCard = new Transform[length];

        for (int i = 0; i < length; ++i)
        {
            characterCard[i] = FindTarget((Character)i, characterCardParent);
        }
    }

    private Transform FindTarget(Character character, Transform parent)
    {
        string characterName = character.ToString();

        for (int i = 0; i < parent.childCount; ++i)
        {
            if (parent.GetChild(i).name.Equals(characterName))
            {
                return parent.GetChild(i);
            }
        }

        return null;
    }

    public void SetJobCard (Transform jobCardParent)
    {
        int length = jobCardParent.childCount;
        jobCard = new Transform[length];

        for (int i = 0; i < length; ++i)
        {
            jobCard[i] = jobCardParent.GetChild(i);
        }
    }
}