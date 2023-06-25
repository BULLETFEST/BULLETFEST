using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GunSpawner : NetworkBehaviour
{
  public List<GameObject> weapons;

  public GameObject goldenGun;

  // const float spawnInterval = 8f;

  // Percent chance (1.0f = 10%, 2.5f = 25%, etc)
  private const float spawnChance = 7f;
  private BoxCollider2D boxCollider2D;
  private readonly float[] spawnMinMax = new float[2];
  private bool firstRound = true;

  private void Start()
  {
    // weapons = Resources.LoadAll<GameObject>("Spawnable/Weapons").ToList();

    // goldenGun = weapons.Where(x => x.name == "Golden Gun").ToArray()[0];

    // weapons.Remove(goldenGun);

    boxCollider2D = GetComponent<BoxCollider2D>();

    float bcSize = boxCollider2D.bounds.extents.x;

    spawnMinMax[0] = transform.position.x - bcSize;
    spawnMinMax[1] = transform.position.x + bcSize;

    _ = StartCoroutine(SpawnWeapon());
  }

  [ServerCallback]
  private IEnumerator SpawnWeapon()
  {
    // while (true)
    // {
    float rndNum = float.Parse(Random.Range(0.0f, 10.0f).ToString()[..3]);

    if (rndNum >= 10f - spawnChance || firstRound)
    {
      firstRound = false;
      // if ()
      GameObject spawnedGun = MyNetworkManager.instance.settings.goldenGun
        ? Instantiate(goldenGun,
                     new Vector2(Random.Range(spawnMinMax[0], spawnMinMax[1]), transform.position.y),
                     Quaternion.Euler(0, 0, 0))
        : Instantiate(weapons[Random.Range(0, weapons.Count)],
                                            new Vector2(Random.Range(spawnMinMax[0], spawnMinMax[1]), transform.position.y),
                                            Quaternion.Euler(0, 0, 0));
      spawnedGun.GetComponent<Rigidbody2D>().AddTorque(Random.Range(0, 1) == 0 ? Random.Range(-20f, -80f) : Random.Range(20f, 80f));

      // spawnedGun.GetComponent<Rigidbody2D>().angularVelocity = 10.0f;
      NetworkServer.Spawn(spawnedGun);
    }

    yield return new WaitForSecondsRealtime(1.3f);
    // yield return new WaitForSecondsRealtime(spawnInterval);
    _ = StartCoroutine(SpawnWeapon());
    // }
  }
}