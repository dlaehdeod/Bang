using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCanvas : MonoBehaviour
{
    public bool isPlaying;

    public Transform equipCard;
    public Transform equipFrontCard;
    public Transform lifeCard;
    public Transform gameCard;
    public Transform area;
    public Transform attackedCard;
    public Transform center;
    public Text nameText;

    public Character character;
    public Job job;

    public int defaultDistance;
    public int mustangDistance;

    public int defaultRange;
    public int gunRange;
    public int mirinoRange;

    private int defaultLayer;
    private int cardLayer;
    private int showCardLayer;
    private float moveSpeed;
    private float delayTime;
    private float cardWidth;

    public delegate bool CardOpenCheck(params Transform[] target);

    private void Awake()
    {
        isPlaying = true;

        Transform playerGround = transform.Find("PlayerGround");
        equipCard = playerGround.Find("Equip Cards");
        equipFrontCard = playerGround.Find("Equip Front Cards");
        lifeCard = playerGround.Find("Life Cards");
        nameText = transform.Find("Name BackGround").GetChild(0).GetComponent<Text>();
        gameCard = transform.Find("PlayerCards");
        area = transform.Find("Area");
        attackedCard = transform.Find("Attacked Card");

        defaultLayer = LayerMask.NameToLayer("UI");
        cardLayer = LayerMask.NameToLayer("Card");
        showCardLayer = LayerMask.NameToLayer("Show Card");

        moveSpeed = 0.025f;
        delayTime = 0.5f;
        cardWidth = 80.0f;

        defaultDistance = 0;
        mustangDistance = 0;
        defaultRange = 0;
        gunRange = 1;
        mirinoRange = 0;
    }

    public void SetBasePlayerPosition()
    {
        equipCard.parent.localPosition = new Vector3(0.0f, -300.0f, 0.0f);
        gameCard.localPosition = new Vector3(0.0f, -430.0f, 0.0f);
        nameText.transform.parent.localPosition = new Vector3(0.0f, -515.0f, 0.0f);
        attackedCard.localPosition = new Vector3(0.0f, -120.0f, 0.0f);
    }

    public void SetJobCard(Transform target, int value)
    {
        job = (Job)value;

        Vector3 jobPos = new Vector3(-80.0f, 0.0f, 0.0f);
        target.SetParent(equipCard);
        StartCoroutine(CardMove(target, jobPos));
    }
    
    private IEnumerator CardMove(Transform target, Vector3 desirePos)
    {
        Vector3 startPos = target.localPosition;
        Quaternion startRot = target.localRotation;
        float moveRatio = 0.0f;

        while (moveRatio < 1.0f)
        {
            yield return new WaitForSeconds(0.01f);
            moveRatio += moveSpeed;
            target.localPosition = Vector3.Lerp(startPos, desirePos, moveRatio);
            target.localRotation = Quaternion.Lerp(startRot, Quaternion.identity, moveRatio);
        }
    }

    public void SetCharacterCard(Transform target, int index, int value)
    {
        character = (Character)value;

        if (character == Character.RoseDoolan)
        {
            defaultRange = 1;
        }
        else if (character == Character.PaulRegret)
        {
            defaultDistance = 1;
        }

        target.SetParent(equipCard);
        StartCoroutine(CardMoveAndSendFinish(target, Vector3.zero, index));
    }

    private IEnumerator CardMoveAndSendFinish(Transform target, Vector3 desirePos, int index)
    {
        yield return StartCoroutine(CardMove(target, desirePos));

        if (BangClient.playerNumber == index)
        {
            ToServer.SendToServer(Header.SetPlayerCharacterCompleted, BangClient.playerNumber);
        }
    }

    public void SetLifeCard(int life, int index)
    {
        StartCoroutine(LifeCardOn(life, index));
    }
    
    private IEnumerator LifeCardOn(int life, int index)
    {
        for (int i = 0; i < life; ++i)
        {
            yield return new WaitForSeconds(0.5f);
            lifeCard.GetChild(i).gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(0.25f);
        if (BangClient.playerNumber == index)
        {
            ToServer.SendToServer(Header.SetPlayerLifeCompleted, BangClient.playerNumber);
        }
    }

    public void SetPlayerCard(int index, Transform[] target)
    {
        StartCoroutine(SetPlayerCardAndSendFinish(target, index));
    }

    private IEnumerator SetPlayerCardAndSendFinish(Transform[] target, int index)
    {
        for (int i = 0; i < target.Length - 1; ++i)
        {
            StartCoroutine(DelayDrawCardMove(target[i], index, i * delayTime));
        }

        yield return StartCoroutine(DelayDrawCardMove(target[target.Length - 1], index, (target.Length - 1) * delayTime));

        if (BangClient.playerNumber == index)
        {
            ToServer.SendToServer(Header.SetPlayerCardCompleted, BangClient.playerNumber);
        }
    }

    public void DrawCard(int index, params Transform[] target)
    {
        for (int i = 0; i < target.Length; ++i)
        {
            StartCoroutine(DelayDrawCardMove(target[i], index, delayTime * (i + 1)));
        }
    }

    private IEnumerator DelayDrawCardMove(Transform target, int index, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        target.SetParent(gameCard);
        target.gameObject.layer = cardLayer;
        int sortCount = gameCard.childCount;

        yield return StartCoroutine(CardMove(target, Vector3.zero));

        if (BangClient.playerNumber == index)
        {
            target.GetChild(0).gameObject.SetActive(false);
        }

        PlayerGameCardSort(sortCount);
    }

    public void PlayerGameCardSort(int count)
    {
        if (count <= 0)
        {
            return;
        }

        if (count <= 5)
        {
            gameCard.GetChild(0).localPosition = new Vector3(-cardWidth * 0.5f * (count - 1), 0.0f, 0.0f);
            for (int i = 1; i < count; ++i)
            {
                gameCard.GetChild(i).localPosition = new Vector3(gameCard.GetChild(i - 1).localPosition.x + cardWidth, 0.0f, 0.0f);
            }
        }
        else
        {
            float offset = (cardWidth * 4.0f) / (count - 1);
            gameCard.GetChild(0).localPosition = new Vector3(-cardWidth * 2.0f, 0.0f, 0.0f);

            for (int i = 1; i < count; ++i)
            {
                gameCard.GetChild(i).localPosition = new Vector3(gameCard.GetChild(i - 1).localPosition.x + offset, 0.0f, 0.0f);
            }
        }
    }

    public void BlackJackDrawAndCardOpen(int index, Transform parent, Transform[] target)
    {
        StartCoroutine(DelayDrawCardMove(target[0], index, 0.0f));
        StartCoroutine(BlackJackCardOpen(target[1], parent, index));
    }

    private IEnumerator BlackJackCardOpen(Transform target, Transform parent, int index)
    {
        yield return new WaitForSeconds(1.0f);
        target.SetParent(parent);

        float size = 1.0f;
        while (size < 2.5f)
        {
            yield return new WaitForSeconds(0.01f);
            size += 0.05f;
            target.localScale = Vector3.one * size;
        }

        target.GetChild(0).gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        if (BangClient.playerNumber == index)
        {
            SendCardOpenMessage(target);
        }

        yield return new WaitForSeconds(1.0f);

        target.GetChild(0).gameObject.SetActive(true);

        while (size > 1.0f)
        {
            yield return new WaitForSeconds(0.01f);
            size -= 0.05f;
            target.localScale = Vector3.one * size;
        }

        target.SetParent(target.transform);
        StartCoroutine(DelayDrawCardMove(target, index, 0.0f));
    }

    private void SendCardOpenMessage(Transform target)
    {
        if (target.name[0] == 'H' || target.name[0] == 'D')
        {
            ToServer.SendToServer(Header.BlackJackDrawMore, 1);
        }
        else
        {
            ToServer.SendToServer(Header.BlackJackDrawMore, 0);
        }
    }

    public void KitCarlsonRestoreCard(Transform target, Transform parent)
    {
        target.SetParent(parent);
        target.gameObject.layer = defaultLayer;
        target.GetChild(0).gameObject.SetActive(true);

        StartCoroutine(CardMove(target, Vector3.zero));

        PlayerGameCardSort(gameCard.childCount);
    }

    public void StealCard(int index, Transform target)
    {
        target.SetParent(gameCard);
        StartCoroutine(DelayDrawCardMove(target, index, 0.0f));
    }

    public void BangCardOpen(int targetIndex, int damage, Transform[] target, Transform parent)
    {
        StartCoroutine(BangCardOpenAndSendResult(targetIndex, damage, target, parent));
    }

    private IEnumerator BangCardOpenAndSendResult(int targetIndex, int damage, Transform[] target, Transform parent)
    {
        yield return StartCoroutine(CardOpenAnimation(target, parent, CardOpenHeartCheck));

        yield return new WaitForSeconds(0.5f);

        if (BangClient.playerNumber == targetIndex)
        {
            ToServer.SendToServer(Header.BangCardOpenCompleted, targetIndex, damage);
        }
    }

    private IEnumerator CardOpenAnimation(Transform[] target, Transform parent, CardOpenCheck cardOpenCheck)
    {
        for (int i = 0; i < target.Length; ++i)
        {
            yield return new WaitForSeconds(1.0f);
            target[i].GetChild(0).gameObject.SetActive(false);
            target[i].SetParent(center);

            float size = 1.0f;
            while (size < 2.5f)
            {
                yield return new WaitForSeconds(0.01f);
                size += 0.05f;
                target[i].localScale = Vector3.one * size;
            }

            yield return new WaitForSeconds(0.5f);

            string message = "실패";

            if (cardOpenCheck(target[i]))
            {
                message = "성공!";
            }

            InformationView.instance.ShowMessage(message, true);

            yield return new WaitForSeconds(1.0f);

            while (size > 1.0f)
            {
                yield return new WaitForSeconds(0.01f);
                size -= 0.05f;
                target[i].localScale = Vector3.one * size;
            }

            target[i].localScale = Vector3.one;
            target[i].SetParent(parent);
            StartCoroutine(CardMove(target[i], Vector3.zero));
        }
    }

    public void BangCardDoubleOpen(int targetIndex, int damage, Transform[] target, Transform parent)
    {
        StartCoroutine(BangCardDoubleOpenAndSendResult(targetIndex, damage, target, parent));
    }

    private IEnumerator BangCardDoubleOpenAndSendResult(int targetIndex, int damage, Transform[] target, Transform parent)
    {
        yield return StartCoroutine(CardDoubleOpenAnimation(target, parent, CardOpenHeartCheck));

        yield return new WaitForSeconds(0.5f);

        if (BangClient.playerNumber == targetIndex)
        {
            ToServer.SendToServer(Header.BangCardOpenCompleted, targetIndex, damage);
        }
    }

    private IEnumerator CardDoubleOpenAnimation(Transform[] target, Transform parent, CardOpenCheck cardOpenCheck)
    {
        yield return new WaitForSeconds(1.0f);
        target[0].GetChild(0).gameObject.SetActive(false);
        target[0].SetParent(center);
        target[1].GetChild(0).gameObject.SetActive(false);
        target[1].SetParent(center);

        float size = 1.0f;
        float moveRatio = 0.0f;
        Vector2 originPos = target[0].localPosition;
        Vector2 targetPos1 = originPos + (Vector2.left * 100.0f);
        Vector2 targetPos2 = originPos + (Vector2.right * 100.0f);

        while (size < 2.5f)
        {
            yield return new WaitForSeconds(0.01f);
            size += 0.05f;
            moveRatio += 0.0334f;

            target[0].localScale = Vector3.one * size;
            target[1].localScale = Vector3.one * size;

            target[0].localPosition = Vector2.Lerp(originPos, targetPos1, moveRatio);
            target[1].localPosition = Vector2.Lerp(originPos, targetPos2, moveRatio);
        }

        yield return new WaitForSeconds(0.5f);

        string message = "실패";

        if (cardOpenCheck(target))
        {
            message = "성공!";
        }

        InformationView.instance.ShowMessage(message, true);

        yield return new WaitForSeconds(1.0f);

        while (size > 1.0f)
        {
            yield return new WaitForSeconds(0.01f);
            size -= 0.05f;
            target[0].localScale = Vector3.one * size;
            target[1].localScale = Vector3.one * size;
        }

        target[0].localScale = Vector3.one;
        target[0].SetParent(parent);
        target[1].localScale = Vector3.one;
        target[1].SetParent(parent);

        StartCoroutine(CardMove(target[0], Vector3.zero));
        StartCoroutine(CardMove(target[1], Vector3.zero));
    }

    private bool CardOpenHeartCheck(params Transform[] target)
    {
        for (int i = 0; i < target.Length; ++i)
        {
            if (target[i].name[0] == 'H')
            {
                return true;
            }
        }

        return false;
    }

    private bool CardOpenDinamiteCheck(params Transform[] target)
    {
        int dinamiteHit = 0;

        for (int i = 0; i < target.Length; ++i)
        {
            string cardName = target[i].name;

            if (cardName[0] == 'S' && '2' <= cardName[1] && cardName[1] <= '9')
            {
                dinamiteHit++;
            }
        }

        return dinamiteHit < target.Length;
    }

    public void EquipAttackCard(Transform target)
    {
        target.SetParent(attackedCard);
        target.GetChild(0).gameObject.SetActive(false);

        StartCoroutine(CardMove(target, Vector3.zero));
    }

    public void EquipFrontCard(Transform target)
    {
        target.SetParent(equipFrontCard);
        target.gameObject.layer = showCardLayer;
        target.GetChild(0).gameObject.SetActive(false);

        StartCoroutine(EquipCardMove(target));

        PlayerGameCardSort(gameCard.childCount);
    }

    public void EquipMiddleCard(Transform target)
    {
        target.SetParent(equipCard);
        target.gameObject.layer = showCardLayer;
        target.GetChild(0).gameObject.SetActive(false);

        StartCoroutine(CardMove(target, new Vector3(80.0f, 0.0f, 0.0f)));

        PlayerGameCardSort(gameCard.childCount);
    }
    
    public void DropCard(Transform target, Transform newParent)
    {
        target.SetParent(newParent);
        target.gameObject.layer = showCardLayer;
        StartCoroutine(CardMove(target, Vector3.zero));

        target.GetChild(0).gameObject.SetActive(false);

        PlayerGameCardSort(gameCard.childCount);
        EquipFrontCardSort();
    }

    private IEnumerator EquipCardMove(Transform target)
    {
        yield return StartCoroutine(CardMove(target, Vector3.zero));
        EquipFrontCardSort();
    }

    public void EquipFrontCardSort()
    {
        int childCount = equipFrontCard.childCount;

        if (childCount == 0)
        {
            return;
        }

        if (childCount <= 3)
        {
            float cardWidth = 80;
            equipFrontCard.GetChild(0).localPosition = new Vector3(-cardWidth * 0.5f * (childCount - 1), 0.0f, 0.0f);
            for (int i = 1; i < childCount; ++i)
            {
                equipFrontCard.GetChild(i).localPosition = new Vector3(equipFrontCard.GetChild(i - 1).localPosition.x + cardWidth, 0.0f, 0.0f);
            }
        }
        else
        {
            float cardInterval = 180 / (childCount - 1);
            equipFrontCard.GetChild(0).localPosition = new Vector3(-90.0f, 0.0f, 0.0f);
            for (int i = 1; i < childCount; ++i)
            {
                equipFrontCard.GetChild(i).localPosition = new Vector3(equipFrontCard.GetChild(i - 1).localPosition.x + cardInterval, 0.0f, 0.0f);
            }
        }
    }

    public void PrigioneCardOpen(int playerIndex, Transform[] target, Transform usedCard)
    {
        if (target.Length == 1)
        {
            StartCoroutine(PrigioneCardOpenAndSendResult(playerIndex, target, usedCard));
        }
        else
        {
            StartCoroutine(PrigioneCardDoubleOpenAndSendResult(playerIndex, target, usedCard));
        }
    }

    private IEnumerator PrigioneCardOpenAndSendResult(int playerIndex, Transform[] target, Transform usedCard)
    {
        yield return StartCoroutine(CardOpenAnimation(target, usedCard, CardOpenHeartCheck));

        yield return new WaitForSeconds(0.5f);

        if (BangClient.playerNumber == playerIndex)
        {
            ToServer.SendToServer(Header.PrigioneCardOpenCompleted, playerIndex);
        }
    }

    private IEnumerator PrigioneCardDoubleOpenAndSendResult(int playerIndex, Transform[] target, Transform usedCard)
    {
        yield return StartCoroutine(CardDoubleOpenAnimation(target, usedCard, CardOpenHeartCheck));

        yield return new WaitForSeconds(0.5f);

        if (BangClient.playerNumber == playerIndex)
        {
            ToServer.SendToServer(Header.PrigioneCardOpenCompleted, playerIndex);
        }
    }

    public void DinamiteCardOpen(int playerIndex, int dinamiteExplosion, Transform[] target, Transform usedCard)
    {
        if (target.Length == 1)
        {
            StartCoroutine(DinamiteCardOpenAndSendResult(playerIndex, dinamiteExplosion, target, usedCard));
        }
        else
        {
            StartCoroutine(DinamiteCardDoubleOpenAndSendResult(playerIndex, dinamiteExplosion, target, usedCard));
        }
    }

    private IEnumerator DinamiteCardOpenAndSendResult(int playerIndex, int dinamiteExplosion, Transform[] target, Transform usedCard)
    {
        yield return StartCoroutine(CardOpenAnimation(target, usedCard, CardOpenDinamiteCheck));

        yield return new WaitForSeconds(0.5f);

        if (BangClient.playerNumber == playerIndex)
        {
            ToServer.SendToServer(Header.DinamiteCardOpenCompleted, playerIndex, dinamiteExplosion);
        }
    }

    private IEnumerator DinamiteCardDoubleOpenAndSendResult(int playerIndex, int dinamiteExplosion, Transform[] target, Transform usedCard)
    {
        yield return StartCoroutine(CardDoubleOpenAnimation(target, usedCard, CardOpenDinamiteCheck));

        yield return new WaitForSeconds(0.5f);

        if (BangClient.playerNumber == playerIndex)
        {
            ToServer.SendToServer(Header.DinamiteCardOpenCompleted, playerIndex, dinamiteExplosion);
        }
    }

    public void SetArea(bool value)
    {
        area.gameObject.SetActive(value);
    }

    public int GetPanicoRange ()
    {
        return defaultRange + mirinoRange;
    }

    public int GetRange ()
    {
        return defaultRange + gunRange + mirinoRange;
    }

    public int GetDistance ()
    {
        return defaultDistance + mustangDistance;
    }

    public void ShowDefeatMessage ()
    {
        transform.Find("Defeat Text").gameObject.SetActive(true);
    }

    public void HideDefeatMessage ()
    {
        transform.Find("Defeat Text").gameObject.SetActive(false);
    }

    public int GetAllCard(int index)
    {
        int count = equipFrontCard.childCount + gameCard.childCount;

        if (equipCard.childCount >= 3)
        {
            count++;
        }

        return count;
    }

    public void Defeat (int[] card, Transform usedCard, Transform characterCardParent)
    {
        ShowDefeatMessage();
        StartCoroutine(DelayCardDrop(card, usedCard, characterCardParent));
    }

    private IEnumerator DelayCardDrop (int[] card, Transform usedCard, Transform characterCardParent)
    {
        Transform target;

        if (card != null)
        {
            for (int i = 0; i < card.Length; ++i)
            {
                target = CardTransform.instance.playCard[card[i]];

                target.gameObject.layer = showCardLayer;
                target.GetChild(0).gameObject.SetActive(false);
                target.SetParent(usedCard);

                yield return new WaitForSeconds(0.25f);
                StartCoroutine(CardMove(target, Vector3.zero));
            }
        }

        target = equipCard.GetChild(0);
        target.SetParent(characterCardParent);
        StartCoroutine(CardMove(target, Vector3.zero));
    }

    
    public void UpdateLife (int life)
    {
        for (int i = 0; i < life; ++i)
        {
            lifeCard.GetChild(i).gameObject.SetActive(true);
        }
        for (int i = life; i < 5; ++i)
        {
            lifeCard.GetChild(i).gameObject.SetActive(false);
        }
    }
}