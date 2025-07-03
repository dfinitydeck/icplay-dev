import axios from 'axios'
const instance = axios.create({
  timeout: 60000
})

instance.interceptors.response.use(
  (response) => {
    if (response.data.code === 0) {
      return response.data
    }

    return Promise.reject(response.data)
  },
  (error) => {
    console.error(error)
    return Promise.reject(error)
  }
)

export default instance
