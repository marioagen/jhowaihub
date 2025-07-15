// Queue to keep track of chunks to upload
let chunkQueue = []
let isUploading = false

// Receives the upload component event to start uploading in the background
addEventListener('message', (event) => {
    const fileData = event.data.message
    chunkQueue.push(fileData)
    self.postMessage({
        type: 'uploadStarted',
        success: true,
        namesFiles: fileData.message.additionalData.filesNames,
    })
    processQueue()
})

// Processes the chunks in the queue
async function processQueue() {
    if (isUploading || chunkQueue.length === 0) {
        return
    }
    isUploading = true
    const fileData = chunkQueue.shift()
    const {
        fileChunk,
        fileType,
        additionalData,
        headers,
        fileName,
        userEmail,
        tokenAzure,
        url,
        chunkIndex,
        totalChunks,
    } = fileData.message
    const chunk = new Blob([fileChunk], { type: fileType })

    await uploadChunk(
        chunk,
        additionalData,
        headers,
        fileName,
        userEmail,
        tokenAzure,
        url,
        chunkIndex,
        totalChunks
    )

    isUploading = false
    processQueue()
}

// Sends the files to the back-end
async function uploadChunk(
    chunk,
    additionalData,
    headers,
    fileName,
    userEmail,
    tokenAzure,
    url,
    chunkIndex,
    totalChunks
) {
    const formData = new FormData()
    formData.append('chunk', chunk, `${fileName}.part${chunkIndex + 1}`)
    formData.append('filename', fileName)
    formData.append('isLast', chunkIndex === totalChunks - 1)
    formData.append('name', additionalData.name)
    formData.append('description', additionalData.description)
    formData.append('emailCreator', additionalData.emailCreator)

    const fullURL = url + '/api/Document/UploadByChunks'

    try {
        const response = await fetch(fullURL, {
            method: 'POST',
            body: formData,
            headers: headers,
        })

        if (response.ok) {
            if (chunkIndex < totalChunks - 1) {
                self.postMessage({
                    type: 'uploadInProgress',
                    success: true,
                    nameFile: fileName,
                    chunkIndex: chunkIndex + 1,
                    chunks: totalChunks,
                })
            } else if (chunkIndex === totalChunks - 1) {
                self.postMessage({
                    type: 'uploadComplete',
                    success: true,
                    nameFile: fileName,
                    chunkIndex: chunkIndex + 1,
                    chunks: totalChunks,
                })
            }
        } else if (response.status === 401) {
            try {
                const result = await renewToken(userEmail, tokenAzure, url)
                headers['Authorization'] = `Bearer ${result.tokenApi}`
                await uploadChunk(
                    chunk,
                    additionalData,
                    headers,
                    fileName,
                    userEmail,
                    tokenAzure,
                    url,
                    chunkIndex,
                    totalChunks
                )
            } catch (error) {
                console.error(`Erro ao renovar o token: ${error.message}`)
            }
        } else {
            self.postMessage({
                type: 'uploadComplete',
                success: false,
                nameFile: fileName,
                chunkIndex: chunkIndex + 1,
                chunks: totalChunks,
            })
            console.error(`Erro ao enviar o chunk ${chunkIndex + 1}: ${response.statusText}`)
        }
    } catch (error) {
        self.postMessage({
            type: 'uploadComplete',
            success: false,
            nameFile: fileName,
            chunkIndex: chunkIndex + 1,
            chunks: totalChunks,
        })
        console.error(`Erro ao enviar o chunk ${chunkIndex + 1}:`, error)
    }
}

// Renews the token used in the request
async function renewToken(userEmail, tokenAzure, url) {
    const authenticateDto = {
        login: userEmail,
    }

    const fullURL = url + '/api/Account/Authenticate'
    try {
        const response = await fetch(fullURL, {
            method: 'POST',
            headers: {
                Authorization: `Bearer ${tokenAzure}`,
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(authenticateDto),
        })

        if (!response.ok) {
            throw new Error(`Erro na autentica��o: ${response.statusText}`)
        }

        const data = await response.json()
        return {
            tokenApi: data.token,
            tenant: data.tenant,
            keyMongoAccess: data.keyMongoAccess,
        }
    } catch (error) {
        throw new Error(`Erro na autentica��o: ${error}`)
    }
}
