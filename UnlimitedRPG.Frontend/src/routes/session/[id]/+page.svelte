<script lang="ts">
	import { getSessionMessages, addSessionMessage } from '$lib/api';
	import type { PageData } from './$types';

	type Mode = 'say' | 'do';

	interface Message {
		id: string;
		role: 'player' | 'game';
		mode: Mode | '';
		text: string;
	}

	let { data }: { data: PageData } = $props();

	let messages = $state<Message[]>([]);
	let input = $state('');
	let mode = $state<Mode>('say');
	let sending = $state(false);
	let error = $state('');

	$effect(() => {
		getSessionMessages(data.id)
			.then((msgs) => {
				messages = msgs.map((m) => ({
					id: m.id,
					role: m.role as 'player' | 'game',
					mode: m.mode as Mode | '',
					text: m.text
				}));
			})
			.catch(() => (error = 'Could not load session history.'));
	});

	async function send() {
		const text = input.trim();
		if (!text || sending) return;
		sending = true;
		error = '';
		try {
			const res = await addSessionMessage(data.id, mode, text);
			messages.push({
				id: res.playerMessage.id,
				role: 'player',
				mode: res.playerMessage.mode as Mode,
				text: res.playerMessage.text
			});
			messages.push({
				id: res.gameMessage.id,
				role: 'game',
				mode: '',
				text: res.gameMessage.text
			});
			input = '';
		} catch {
			error = 'Failed to send message.';
		} finally {
			sending = false;
		}
	}
</script>

<div class="flex flex-col h-screen">
	<!-- Header -->
	<header class="border-b px-4 py-3 flex items-center gap-3">
		<a href="/" class="text-sm hover:underline">← Back</a>
		<span class="text-sm text-gray-500">Session {data.id.slice(0, 8)}…</span>
	</header>

	<!-- Message history -->
	<div class="flex-1 overflow-y-auto px-4 py-4 space-y-3">
		{#if messages.length === 0}
			<p class="text-sm text-gray-400 text-center mt-8">Your adventure begins. Say or do something.</p>
		{/if}
		{#each messages as msg (msg.id)}
			{#if msg.role === 'player'}
				<div class="flex flex-col items-end gap-0.5">
					<span class="text-xs text-gray-400">{msg.mode === 'say' ? 'Say' : 'Do'}</span>
					<p class="text-sm text-gray-900 bg-gray-100 rounded px-3 py-2 max-w-prose">{msg.text}</p>
				</div>
			{:else}
				<div class="flex flex-col items-start gap-0.5">
					<span class="text-xs text-gray-400">Narrator</span>
					<p class="text-sm text-gray-900 bg-blue-50 rounded px-3 py-2 max-w-prose">{msg.text}</p>
				</div>
			{/if}
		{/each}
		{#if error}
			<p class="text-sm text-red-500 text-center">{error}</p>
		{/if}
	</div>

	<!-- Input area -->
	<div class="border-t px-4 py-3 space-y-2">
		<!-- Mode toggle -->
		<div class="flex gap-1">
			<button
				type="button"
				onclick={() => (mode = 'say')}
				class="px-3 py-1 text-sm rounded border {mode === 'say' ? 'bg-gray-900 text-white border-gray-900' : 'border-gray-300'}"
			>
				Say something
			</button>
			<button
				type="button"
				onclick={() => (mode = 'do')}
				class="px-3 py-1 text-sm rounded border {mode === 'do' ? 'bg-gray-900 text-white border-gray-900' : 'border-gray-300'}"
			>
				Do something
			</button>
		</div>

		<!-- Text input + send -->
		<form
			onsubmit={(e) => { e.preventDefault(); send(); }}
			class="flex gap-2"
		>
			<input
				type="text"
				bind:value={input}
				placeholder={mode === 'say' ? 'Say something…' : 'Do something…'}
				class="flex-1 rounded border px-3 py-2 text-sm"
			/>
			<button
				type="submit"
				disabled={!input.trim() || sending}
				class="rounded border px-4 py-2 text-sm font-medium disabled:opacity-40"
			>
				{sending ? 'Sending…' : 'Send'}
			</button>
		</form>
	</div>
</div>
