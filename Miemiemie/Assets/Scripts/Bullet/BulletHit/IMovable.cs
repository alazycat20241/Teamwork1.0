// IMovable.cs
public interface IMovable
{
    //减速
    float GetMoveSpeed();
    void SetMoveSpeed(float speed);

    //击退
    void StartKnockback();   // 
    void EndKnockback();     //

    /// 暂停移动
    void PauseMovement();

    /// 恢复移动
    void ResumeMovement();
}