// Load configuration in async function
async function loadConfig(key) {
  const config =
    import.meta.env.MODE === 'production'
      ? await import('@/config/prod.config')
      : await import('@/config/dev.config')
  return key ? config[key] : config
}

export { loadConfig }
