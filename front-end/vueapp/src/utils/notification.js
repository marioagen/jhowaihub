export function notify({ 
    title, 
    message, 
    variant = 'primary', 
    icon = null, 
    duration = 3000 
}) {
    const event = new CustomEvent('notification:show', {
        detail: { title, message, variant, icon, duration }
    })
    window.dispatchEvent(event)
}