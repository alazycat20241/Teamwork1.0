// IMovable.cs
public interface IMovable
{
    //减速
    float GetMoveSpeed();
    void SetMoveSpeed(float speed);

    //击退
    void StartKnockback();   // 
    void EndKnockback();     //
}