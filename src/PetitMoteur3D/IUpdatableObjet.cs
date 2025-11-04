namespace PetitMoteur3D;

internal interface IUpdatableObjet
{
    /// <summary>
    /// Update l'objet
    /// </summary>
    /// <param name="elapsedTime"></param>
    void Update(float elapsedTime);
}
