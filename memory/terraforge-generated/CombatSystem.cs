using UnityEngine;

public class CombatSystem : MonoBehaviour
{
 public float meleeDamage = 10f;
 public float rangedDamage = 20f;
 public float attackRange = 5f;
 public float projectileSpeed = 10f;
 public float criticalHitChance = 0.1f;
 private float meleeCooldown = 0f;
 private float rangedCooldown = 0f;

 void Update()
 {
 if (Input.GetKeyDown(KeyCode.E) && meleeCooldown <= 0)
 {
 MeleeAttack();
 }
 else if (Input.GetKeyDown(KeyCode.Mouse0) && rangedCooldown <= 0)
 {
 RangedAttack();
 }

 meleeCooldown -= Time.deltaTime;
 rangedCooldown -= Time.deltaTime;
 }

 void MeleeAttack()
 {
 Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
 foreach (var collider in hitColliders)
 {
 if (collider.CompareTag("Enemy"))
 {
 float damage = CalculateDamage(meleeDamage);
 if (Random.value < criticalHitChance)
 {
 damage *= 2;
 }
 collider.GetComponent<Health>().TakeDamage(damage);
 }
 }
 meleeCooldown = 1f;
 }

 void RangedAttack()
 {
 GameObject projectile = Instantiate(new GameObject(), transform.position, Quaternion.identity);
 Rigidbody rb = projectile.AddComponent<Rigidbody>();
 rb.velocity = transform.forward * projectileSpeed;
 Destroy(projectile, 2f);
 rangedCooldown = 2f;
 }

 float CalculateDamage(float baseDamage)
 {
 return baseDamage;
 }
}
