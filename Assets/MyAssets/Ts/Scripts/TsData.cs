using System;
using UnityEngine;

public class TsData
{
    public Sprite SpriteResource { get; }   //�Z���̔w�i�e�N�X�`����ǂݍ��ނ��߂̃L�[�̃A�N�Z�T
    public string Text { get; }             // �Z���ɕ\�����镶����̃A�N�Z�T
    public Action Clicked { get; }          // �Z�����N���b�N���ꂽ���Ɏ��s����A�N�V����

    // �R���X�g���N�^
    public TsData(Sprite spriteResource, string text, Action clicked)
    {
        SpriteResource = spriteResource;
        Text = text;
        Clicked = clicked;
    }
}
