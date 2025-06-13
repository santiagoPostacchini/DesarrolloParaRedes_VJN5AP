using UnityEngine;
using UnityEngine.UI;

public class NicknameItem : MonoBehaviour
{
    private Transform _owner;

    private const float HEAD_OFFSET = 2.5F;

    private Text _myText;

    public NicknameItem SetOwner(NetworkPlayer owner)
    {
        _owner = owner.transform;

        _myText = GetComponent<Text>();

        return this;
    }

    public void UpdateText(string nickname)
    {
        _myText.text = nickname;
    }

    public void UpdatePosition()
    {
        transform.position = _owner.position + Vector3.up * HEAD_OFFSET;
    }
}
