import mitt from 'mitt'

const emitter = mitt()

export default {
    emit: emitter.emit,
    on: emitter.on,
    off: emitter.off,
}
