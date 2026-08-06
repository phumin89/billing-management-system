const revealStates = new WeakMap();

export function observeAndReveal(element) {
    const existingState = revealStates.get(element);
    if (existingState) {
        existingState.reveal();
        return;
    }

    const container = element.closest(".sidebar");
    if (!container) {
        return;
    }

    const state = { frame: 0, observer: null, reveal: null };
    const reveal = () => {
        cancelAnimationFrame(state.frame);
        state.frame = requestAnimationFrame(() => {
            if (element.getAttribute("aria-current") !== "page") {
                return;
            }

            const itemBounds = element.getBoundingClientRect();
            const containerBounds = container.getBoundingClientRect();
            if (itemBounds.left < containerBounds.left) {
                container.scrollLeft -= Math.ceil(containerBounds.left - itemBounds.left);
            } else if (itemBounds.right > containerBounds.right) {
                container.scrollLeft += Math.ceil(itemBounds.right - containerBounds.right);
            }
        });
    };
    const observer = new ResizeObserver(reveal);

    state.observer = observer;
    state.reveal = reveal;
    revealStates.set(element, state);
    observer.observe(container);
    for (const child of container.children) {
        observer.observe(child);
    }

    reveal();
}

export function disconnectReveal(element) {
    const state = revealStates.get(element);
    if (!state) {
        return;
    }

    state.observer.disconnect();
    cancelAnimationFrame(state.frame);
    revealStates.delete(element);
}
