#ifndef FASTPBKDF2_H
#define FASTPBKDF2_H

#include <stdlib.h>
#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

/** Calculates PBKDF2-HMAC-SHA1.
 *
 *  @p npw bytes at @p pw are the password input.
 *  @p nsalt bytes at @p salt are the salt input.
 *  @p iterations is the PBKDF2 iteration count and must be non-zero.
 *  @p nout bytes of output are written to @p out.  @p nout must be non-zero.
 *
 *  This function cannot fail; it does not report errors.
 */
void fastpbkdf2_hmac_sha1(const uint8_t *pw, size_t npw,
                          const uint8_t *salt, size_t nsalt,
                          uint32_t iterations,
                          uint8_t *out, size_t nout);

/** Calculates PBKDF2-HMAC-SHA256.
 *
 *  @p npw bytes at @p pw are the password input.
 *  @p nsalt bytes at @p salt are the salt input.
 *  @p iterations is the PBKDF2 iteration count and must be non-zero.
 *  @p nout bytes of output are written to @p out.  @p nout must be non-zero.
 *
 *  This function cannot fail; it does not report errors.
 */
void fastpbkdf2_hmac_sha256(const uint8_t *pw, size_t npw,
                            const uint8_t *salt, size_t nsalt,
                            uint32_t iterations,
                            uint8_t *out, size_t nout);

/** Calculates PBKDF2-HMAC-SHA512.
 *
 *  @p npw bytes at @p pw are the password input.
 *  @p nsalt bytes at @p salt are the salt input.
 *  @p iterations is the PBKDF2 iteration count and must be non-zero.
 *  @p nout bytes of output are written to @p out.  @p nout must be non-zero.
 *
 *  This function cannot fail; it does not report errors.
 */
void fastpbkdf2_hmac_sha512(const uint8_t *pw, size_t npw,
                            const uint8_t *salt, size_t nsalt,
                            uint32_t iterations,
                            uint8_t *out, size_t nout);

#ifdef __cplusplus
}
#endif

#endif
